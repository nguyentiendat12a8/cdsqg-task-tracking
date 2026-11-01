using System.Text.Json;
using Cdsqg.Application.Services;
using Cdsqg.Api.Controllers;
using Cdsqg.Infrastructure.Data;
using Cdsqg.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Xunit;
namespace Cdsqg.Tests;
public sealed class DigitalNewsTests
{
    [Theory]
    [InlineData("https://evil.test/spreadsheets/d/12345678901234567890/edit")]
    [InlineData("http://docs.google.com/spreadsheets/d/12345678901234567890/edit")]
    [InlineData("https://docs.google.com.evil.test/spreadsheets/d/12345678901234567890/edit")]
    [InlineData("https://user@docs.google.com/spreadsheets/d/12345678901234567890/edit")]
    [InlineData("https://docs.google.com/spreadsheets/d/12345678901234567890/edit?gid=hello")]
    public void RejectsInvalidSheetUrls(string url)=>Assert.Throws<InvalidOperationException>(()=>DigitalNewsSheetReader.ValidateSheetUrl(url));
    [Fact]
    public void SheetUrlUsesExplicitTabOrFirstTab()
    {
        var result=DigitalNewsSheetReader.ValidateSheetUrl(DigitalNewsSyncSettings.InitialSheet);Assert.EndsWith("&gid=731708878",result.CsvUrl);
        var first=DigitalNewsSheetReader.ValidateSheetUrl(DigitalNewsSyncSettings.InitialSheet.Split('?')[0]);Assert.DoesNotContain("gid=",first.CsvUrl);
        Assert.EndsWith("&gid=42",DigitalNewsSheetReader.ValidateSheetUrl(first.Url+"#gid=42").CsvUrl);
    }
    [Fact]
    public void CsvHandlesVietnameseCommasMultilineDuplicatesAndUnsafeRows()
    {
        var sheet=DigitalNewsSheetReader.Parse("STT,Tên bộ/tỉnh,Tên bài viết,Link nguồn bài viết,,\n1,Hà Nội,\"Bài viết, dòng 1\ndòng 2\",https://hanoi.gov.vn/a?utm_source=x#top,,\n2,Hà Nội,Tiêu đề sửa,https://hanoi.gov.vn/a,,\n3,Huế,Bài khác,https://hue.gov.vn/b,,\n4,X,Không hợp lệ,javascript:alert(1),,\n5,X,Thiếu link,,,\n,,,,,\n");
        Assert.Equal(2,sheet.Rows.Count);Assert.Equal(3,sheet.Skipped);Assert.Contains(sheet.Rows,r=>r.Title=="Tiêu đề sửa"&&r.Url=="https://hanoi.gov.vn/a");
        Assert.Throws<InvalidOperationException>(()=>DigitalNewsSheetReader.Parse("<html>Đăng nhập</html>"));
        Assert.Throws<InvalidOperationException>(()=>DigitalNewsSheetReader.Parse("Tên bộ/tỉnh,Tên bài viết,Link nguồn bài viết\nX,\"bad,https://hanoi.gov.vn"));
    }
    [PostgresFact]
    public async Task SyncIsIdempotentConcurrentSafeScheduledAndNeverDeletesExistingItems()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior",true);
        var connection=Environment.GetEnvironmentVariable("CDSQG_TEST_POSTGRES")!;var schema="news_"+Guid.NewGuid().ToString("N");
        await using var admin=new NpgsqlConnection(connection);await admin.OpenAsync();await using(var cmd=new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"",admin))await cmd.ExecuteNonQueryAsync();
        try{
            var options=new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(new NpgsqlConnectionStringBuilder(connection){SearchPath=schema}.ConnectionString).Options;
            await using var db=new AppDbContext(options);await db.Database.MigrateAsync();
            var reader=new FixtureReader();var service=new DigitalNewsSyncService(db,reader);
            var result=await service.RunAsync(false,default);Assert.Equal(2,result!.Added);
            var controller=new DigitalNewsController(db,service);
            async Task<string> FirstTitle(string sortBy,string order,int page=1){var response=Assert.IsType<OkObjectResult>(await controller.List(page:page,pageSize:1,sortBy:sortBy,sortOrder:order));var json=JsonSerializer.SerializeToElement(response.Value,new JsonSerializerOptions(JsonSerializerDefaults.Web));return json.GetProperty("items")[0].GetProperty("title").GetString()!;}
            Assert.Equal("Bài A",await FirstTitle("title","asc"));Assert.Equal("Bài B",await FirstTitle("title","desc"));Assert.Equal("Bài B",await FirstTitle("title","asc",2));Assert.Equal("Bài B",await FirstTitle("importedAt","desc"));Assert.Equal("Bài A",await FirstTitle("importedAt","asc"));
            var config=await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync();Assert.Equal(2,config.IntervalHours);Assert.InRange((config.NextRunAt!.Value-config.LastCompletedAt!.Value).TotalHours,1.999,2.001);
            Assert.Null(await service.RunAsync(false,default));Assert.Equal(1,reader.Calls);
            Assert.Equal(2,(await service.RunAsync(true,default))!.Unchanged);
            reader.Csv="Tên bộ/tỉnh,Tên bài viết,Link nguồn bài viết\nHà Nội,Bài cập nhật,https://hanoi.gov.vn/a\nHuế,Bài mới,https://hue.gov.vn/c\n";
            result=await service.RunAsync(true,default);Assert.Equal(1,result!.Added);Assert.Equal(1,result.Updated);Assert.Equal(3,await db.DigitalNewsItems.CountAsync());
            var response=Assert.IsType<OkObjectResult>(await new DigitalNewsController(db,service).List(search:"cập nhật",page:99,pageSize:1));
            var json=JsonSerializer.SerializeToElement(response.Value);Assert.Equal(1,json.GetProperty("total").GetInt32());Assert.Equal(1,json.GetProperty("page").GetInt32());
            reader.Pause=true;reader.Entered=new(TaskCreationOptions.RunContinuationsAsynchronously);reader.Release=new(TaskCreationOptions.RunContinuationsAsynchronously);
            var running=service.RunAsync(true,default);await reader.Entered.Task;
            await using(var otherDb=new AppDbContext(options)){var other=new DigitalNewsSyncService(otherDb,reader);Assert.Null(await other.RunAsync(true,default));await Assert.ThrowsAsync<InvalidOperationException>(()=>other.SaveAsync(DigitalNewsSyncSettings.InitialSheet,false,2,"Admin",default));}
            reader.Release.SetResult();await running;reader.Pause=false;
            reader.Csv="bad header";await Assert.ThrowsAsync<InvalidOperationException>(()=>service.RunAsync(true,default));
            config=await db.DigitalNewsSyncSettings.AsNoTracking().SingleAsync();Assert.Equal("Error",config.LastStatus);Assert.Null(config.LeaseUntil);Assert.Equal(3,await db.DigitalNewsItems.CountAsync());
            await service.SaveAsync(DigitalNewsSyncSettings.InitialSheet,false,4,"Admin",default);Assert.Null(await service.RunAsync(false,default));
            reader.Csv="Tên bộ/tỉnh,Tên bài viết,Link nguồn bài viết\n";Assert.Equal(0,(await service.RunAsync(true,default))!.Added);Assert.Equal(3,await db.DigitalNewsItems.CountAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(()=>service.SaveAsync(DigitalNewsSyncSettings.InitialSheet,true,0,"Admin",default));
        }finally{await using var cmd=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",admin);await cmd.ExecuteNonQueryAsync();}
    }
    private sealed class FixtureReader:IDigitalNewsSheetReader
    {
        public string Csv="STT,Tên bộ/tỉnh,Tên bài viết,Link nguồn bài viết\n1,Hà Nội,Bài A,https://hanoi.gov.vn/a\n2,Huế,Bài B,https://hue.gov.vn/b\n";public int Calls;public bool Pause;public TaskCompletionSource Entered=new(),Release=new();
        public async Task<DigitalNewsSheet> ReadAsync(string url,CancellationToken ct){Calls++;if(Pause){Entered.SetResult();await Release.Task.WaitAsync(ct);}return DigitalNewsSheetReader.Parse(Csv);}
    }
}

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
}

from pathlib import Path
p=Path('frontend/src/views/AiDataView.vue');s=p.read_text(encoding='utf-8-sig');s=s.replace('Ghi chú kiểm chứng hoặc lý do từ chối <span class="text-rose-700">*</span>','Ghi chú kiểm chứng hoặc lý do từ chối (không bắt buộc)').replace('v-model="reviewNote" required minlength="5" maxlength="2000" aria-describedby="ai-review-note-help"','v-model="reviewNote" maxlength="2000"').replace('<p id="ai-review-note-help" class="mt-2 text-slate-600">Bắt buộc, ít nhất 5 ký tự để duyệt hoặc từ chối.</p>','').replace('busy || reviewNote.trim().length<5','busy').replace(' || reviewNote.trim().length<5','');p.write_text(s,encoding='utf-8')
p=Path('backend/Application/AI/AiReviewService.cs');s=p.read_text(encoding='utf-8-sig').replace('string note,string actor','string? note,string actor').replace('if(string.IsNullOrWhiteSpace(note) || note.Trim().Length<5 || note.Length>2000)throw new InvalidOperationException("Nhập ghi chú kiểm chứng/từ chối từ 5 đến 2.000 ký tự.");','note=(note??"").Trim();\n        if(note.Length>2000)throw new InvalidOperationException("Ghi chú tối đa 2.000 ký tự.");');p.write_text(s,encoding='utf-8')
p=Path('backend/Api/Controllers/AiDataController.cs');s=p.read_text(encoding='utf-8-sig').replace('decimal? AcceptedValue,string Note,bool Verified=false','decimal? AcceptedValue,string? Note=null,bool Verified=false');p.write_text(s,encoding='utf-8')
p=Path('backend.Tests/AiDataTests.cs');s=p.read_text(encoding='utf-8-sig').replace('18.3m,"Đối chiếu tài liệu gốc","Admin A"','18.3m,"","Admin A"').replace('Assert.Equal(18.2m,first.Value);','Assert.Equal("",first.ReviewNote);Assert.Equal(18.2m,first.Value);');needle='            Assert.Equal(1,await db.AiDataSuggestions.CountAsync(s=>s.IsCurrent));';s=s.replace(needle,needle+'''
            var rejected=new AiDataSuggestion{DocumentId=document.Id,MetricKey=first.MetricKey,DataYear=2025,Value=17m,Unit="%",Page=3};
            db.Add(rejected);await db.SaveChangesAsync();
            await review.ReviewAsync(rejected.Id,false,null,null,"Admin A",default);
            Assert.Equal("Rejected",rejected.Status);Assert.Equal("",rejected.ReviewNote);
''');p.write_text(s,encoding='utf-8')
p=Path('frontend/tests/ai-review.browser.mjs');s=p.read_text(encoding='utf-8-sig');a=s.index(" await dialog.getByRole('checkbox').check();");b=s.index(" await dialog.getByRole('button',{name:'Xem bản chụp",a);s=s[:a]+''' await dialog.getByRole('checkbox').check();
 assert.equal(await dialog.locator('#ai-review-note').inputValue(),'');
 assert.equal(await dialog.getByRole('button',{name:'Duyệt dữ liệu kiểm chứng'}).isEnabled(),true);
 assert.equal(await dialog.getByRole('button',{name:'Từ chối',exact:true}).isEnabled(),true);
 await dialog.getByRole('checkbox').uncheck();
 assert.equal(await dialog.getByRole('button',{name:'Duyệt dữ liệu kiểm chứng'}).isDisabled(),true);
'''+s[b:];p.write_text(s,encoding='utf-8')

import fs from 'node:fs/promises';
import {Workbook,SpreadsheetFile} from '@oai/artifact-tool';
const goals=JSON.parse((await fs.readFile(new URL('./goals.json',import.meta.url),'utf8')).replace(/^\uFEFF/,''));
if(!goals.some(g=>g.Code==='MT-27'))goals.push({Code:'MT-27',Title:'Quy mô kinh tế số đạt tối thiểu 30% GDP',Agency:'Bộ Khoa học và Công nghệ',archived:true});
const plans={
'01':['Có điều kiện','Báo cáo dịch vụ công, bộ/ngành, địa phương','Bóc tách tỷ lệ số hóa theo từng phạm vi','Phân biệt số hóa đầu vào, kết quả điện tử và toàn bộ hồ sơ','https://mst.gov.vn/chuyen-doi-so-tu-co-so-cao-bang-dua-dich-vu-cong-den-gan-nguoi-dan-197251105150833507.htm'],
'02':['Có điều kiện','Kết quả khảo sát và báo cáo dịch vụ công chính thức','Bóc tách mức hài lòng về dịch vụ công trực tuyến','Không dùng mức hài lòng chung thay cho khảo sát đúng đối tượng',''],
'03':['Hỗ trợ minh chứng','Báo cáo nền tảng số của bộ/ngành và địa phương','Tìm minh chứng triển khai nền tảng','Cần danh mục nền tảng, đơn vị và tiêu chí sử dụng hiệu quả',''],
'04':['Hỗ trợ minh chứng','Báo cáo kết nối và chia sẻ dữ liệu','Tìm văn bản xác nhận kết nối','Cần danh mục đầy đủ các hệ thống để xác định mẫu số',''],
'05':['Hỗ trợ minh chứng','Báo cáo CSDL và kết quả đánh giá của cơ quan quản lý','Đối chiếu điều kiện chuẩn hóa, kết nối, phân quyền, AI','Không suy ra đáp ứng toàn bộ từ việc đã kết nối',''],
'06':['Hỗ trợ minh chứng','Báo cáo vận hành IOC và hồ sơ nghiệm thu','Tìm minh chứng triển khai, nghiệm thu AI','Tin khai trương không chứng minh đủ chức năng và vận hành',''],
'07':['Hỗ trợ minh chứng','Báo cáo quản lý nhân sự công vụ','Bóc tách số hồ sơ điện tử đã đạt yêu cầu','Cần số cán bộ thuộc phạm vi và tiêu chí hồ sơ đạt chuẩn',''],
'08':['Hỗ trợ minh chứng','Báo cáo cơ quan thống kê','Đối chiếu danh mục chỉ tiêu và phương pháp biên soạn','Cần phân biệt sử dụng dữ liệu hành chính và các nguồn khác',''],
'09':['Có điều kiện','Báo cáo đào tạo kỹ năng số của các cơ quan','Bóc tách số người và tỷ lệ đã đào tạo','Không lấy số lớp hoặc lượt tham gia thay số người đủ điều kiện',''],
'10':['Có điều kiện','Báo cáo bồi dưỡng cán bộ chuyên trách','Bóc tách đối tượng chuyên trách và kết quả đào tạo','Đúng đối tượng, thời kỳ và không trùng người',''],
'11':['Hỗ trợ minh chứng','Báo cáo sử dụng trợ lý AI công vụ','Đối chiếu cấp tài khoản và đào tạo','Phải đáp ứng đồng thời cả hai điều kiện',''],
'12':['Ưu tiên','UN E-Government Survey và phụ lục dữ liệu','Lấy thứ hạng EGDI toàn cầu của Việt Nam','Thứ hạng ≤ 50; không dùng điểm EGDI, EPI, OSI, LOSI hay xếp hạng ASEAN','https://desapublications.un.org/publications/un-e-government-survey-2024'],
'13':['Ưu tiên','Cục Thống kê, báo cáo chính thức về kinh tế số','Lấy tỷ trọng kinh tế số trong GDP toàn quốc','≥ 30%; thống nhất phương pháp, kỳ và phiên bản số liệu','https://www.nso.gov.vn/tai-khoan-quoc-gia/'],
'14':['Có điều kiện','Báo cáo chương trình hỗ trợ doanh nghiệp chuyển đổi số','Bóc tách số doanh nghiệp đã hỗ trợ','Thống nhất định nghĩa đã hỗ trợ; tránh đếm trùng doanh nghiệp',''],
'15':['Hỗ trợ minh chứng','Báo cáo vận hành sàn dữ liệu và hồ sơ xác nhận','Tìm sàn đã đưa vào sử dụng và căn cứ pháp lý','Cần tối thiểu 5 sàn đáp ứng an toàn, tuân thủ; đơn vị hiện tại % cần rà soát',''],
'16':['Ưu tiên','Sách trắng và báo cáo TMĐT của Bộ Công Thương','Lấy tỷ trọng doanh thu TMĐT trong tổng bán lẻ và dịch vụ tiêu dùng','Đối chiếu ngưỡng 15–17%; không nhầm tăng trưởng, thị phần sàn hoặc doanh thu','https://moit.gov.vn/upload/2005517/fck/files/BCTMDT2022-final-pdf_08c88.PDF'],
'17':['Hỗ trợ minh chứng','Khảo sát doanh nghiệp và báo cáo ngành thương mại','Bóc tách kết quả khảo sát đúng đối tượng','Cần mẫu số doanh nghiệp nhỏ và vừa trong lĩnh vực thương mại, tránh trùng sàn',''],
'18':['Có điều kiện','NHNN và cơ quan thống kê GDP','Bóc tách giá trị thanh toán, tỷ số với GDP nếu công bố','Cùng năm, đơn vị, phạm vi; đơn vị là lần GDP, không phải %','https://www.sbv.gov.vn/'],
'19':['Hỗ trợ minh chứng','Khảo sát chính thức ứng dụng AI của doanh nghiệp','Tìm minh chứng và kết quả khảo sát','Tin từng doanh nghiệp không suy được tỷ lệ toàn quốc',''],
'20':['Có điều kiện','Bộ KH&CN và báo cáo viễn thông','Thu thập tỷ lệ phủ hạ tầng cáp quang','Phủ hạ tầng khác với hộ đang sử dụng thuê bao cáp quang','https://mst.gov.vn/6-thang-dau-nam-2025-nganh-vien-thong-viet-nam-tiep-tuc-duy-tri-da-tang-truong-on-dinh-197251011003815722.htm'],
'21':['Có điều kiện','NHNN và hệ thống chỉ tiêu thống kê','Lấy tỷ lệ người có tài khoản giao dịch','Đúng từ 15 tuổi, số người duy nhất, không nhầm người trưởng thành hay số tài khoản','https://www.nso.gov.vn/phuong-phap-luan-thong-ke/he-thong-chi-tieu-thong-ke/'],
'22':['Có điều kiện','NEAC, Bộ KH&CN','Thu thập tỷ lệ người có chữ ký số/điện tử cá nhân','Đúng từ 18 tuổi; số chứng thư không đồng nhất số người','https://neac.gov.vn/vi/tin-tuc-su-kien/detail/tin-hoat-dong-neac/dau-an-tang-truong-manh-cua-thi-truong-chu-ky-so-viet-nam-738.htm'],
'23':['Có điều kiện','Bộ Y tế và báo cáo địa phương','Thu thập tỷ lệ người dân đã lập sổ sức khỏe điện tử','Đã lập sổ khác đã tích hợp VNeID hoặc đã có dữ liệu khám chữa bệnh','https://moh.gov.vn/'],
'24':['Có điều kiện','Báo cáo chương trình đào tạo và từng đơn vị','Bóc tách lượt người được đào tạo đúng độ tuổi','Không cộng trùng hoạt động từ nhiều báo; phân biệt lượt và người',''],
'25':['Ưu tiên','UN E-Government Survey và phụ lục dữ liệu','Dùng cùng chuỗi thứ hạng EGDI với MT-12','Thứ hạng ≤ 30; kiểm tra thời hạn riêng của mục tiêu; đơn vị là hạng','https://desapublications.un.org/publications/un-e-government-survey-2024'],
'26':['Ưu tiên','Cục Thống kê, báo cáo chính thức về kinh tế số','Dùng chuỗi tỷ trọng kinh tế số trong GDP','≥ 50%; khác ngưỡng với MT-13 và MT-27, giữ riêng ID mục tiêu','https://www.nso.gov.vn/tai-khoan-quoc-gia/'],
'27':['Ưu tiên','Cục Thống kê, báo cáo chính thức về kinh tế số','Dùng chuỗi tỷ trọng kinh tế số trong GDP','≥ 30%; cùng nội dung MT-13 trong DB local, cần đối chiếu thời hạn/phụ lục trước khi gộp','https://www.nso.gov.vn/tai-khoan-quoc-gia/']
};
const groups=[['1. Ưu tiên triển khai','Ưu tiên',6],['2. Có điều kiện','Có điều kiện',11],['3. Hỗ trợ minh chứng','Hỗ trợ minh chứng',10]];
const wb=Workbook.create();
for(const [name,group,count] of groups){
 const sheet=wb.worksheets.add(name);sheet.showGridLines=false;sheet.tabColor=group==='Ưu tiên'?'#214A80':group==='Có điều kiện'?'#9B6D20':'#64748B';
 const selected=goals.filter(g=>plans[g.Code.slice(-2)]?.[0]===group);if(selected.length!==count)throw Error('Wrong group count');
 sheet.getRange('A2').values=[[`Đánh giá khả năng khai thác dữ liệu AI: ${group}`]];
 sheet.getRange('A3').values=[['Ngày đánh giá: 17/10/2026. Nội dung mục tiêu đối chiếu DB local; URL là nguồn tham khảo, không phải kết quả cập nhật mới nhất.']];
 sheet.getRange('G3').values=[['Số mục tiêu']];sheet.getRange('H3').formulas=[[`=COUNTA(A6:A${count+5})`]];
 const headers=['Mã','Nội dung mục tiêu','Cơ quan chủ trì','Ứng dụng AI','Nguồn ưu tiên','Điều kiện kiểm chứng','Lộ trình','URL tham khảo'];
 sheet.getRange('A5:H5').values=[headers];
 const rows=selected.map(g=>{const v=plans[g.Code.slice(-2)];return [g.Code,g.Title,g.Agency,v[2],v[1],v[3],g.archived?'Bảo lưu từ đánh giá trước; MT-27 không còn trong DB local 17/10/2026':group==='Ưu tiên'?'Giai đoạn 1: kho AI riêng, liên kết ID mục tiêu, admin kiểm chứng':group==='Có điều kiện'?'Giai đoạn 2: chuẩn hóa định nghĩa và phạm vi nguồn':'Giai đoạn 3: thu thập minh chứng, đối chiếu báo cáo',v[4]];});
 sheet.getRange(`A6:H${count+5}`).values=rows;
 sheet.getRange(`A2:H${count+5}`).format.font={name:'Arial',size:10,color:'#182638'};
 sheet.getRange('A2:H2').format.font={name:'Arial',size:14,bold:true,color:'#214A80'};
 sheet.getRange('A3:F3').format.font={name:'Arial',size:10,color:'#526278'};
 sheet.getRange('A2:H3').format.rowHeight=25;
 sheet.getRange('A5:H5').format={fill:'#214A80',font:{name:'Arial',size:10,bold:true,color:'#FFFFFF'},rowHeight:32,wrapText:true,verticalAlignment:'center',horizontalAlignment:'center'};
 sheet.getRange(`A6:H${count+5}`).format.wrapText=true;sheet.getRange(`A6:H${count+5}`).format.verticalAlignment='top';sheet.getRange(`A6:H${count+5}`).format.rowHeight=112;
 const widths=[12,58,29,42,36,55,38,62];for(let i=0;i<8;i++)sheet.getRange(`${String.fromCharCode(65+i)}5:${String.fromCharCode(65+i)}${count+5}`).format.columnWidth=widths[i];
 for(let r=6;r<count+6;r++){if(r%2===0)sheet.getRange(`A${r}:H${r}`).format.fill='#F1F5F9';}
 sheet.getRange(`A6:A${count+5}`).format.font={name:'Arial',size:10,bold:true,color:'#214A80'};
 sheet.freezePanes.freezeRows(5);sheet.freezePanes.freezeColumns(1);
 sheet.tables.add(`A5:H${count+5}`,true,`Goals${count}`);
}
wb.recalculate();
console.log((await wb.inspect({kind:'region',sheetId:groups[0][0],range:'G3:H3',maxChars:500})).ndjson);
const out='D:/CDSQG/Source thử nghiệm theo dõi chiến lược/tài liệu phát triển/Danh_gia_khai_thac_AI_27_muc_tieu.xlsx';
await (await SpreadsheetFile.exportXlsx(wb)).save(out);
for(let i=0;i<groups.length;i++){const preview=await wb.render({sheetName:groups[i][0],range:'A1:H8',scale:1,format:'png'});await fs.writeFile(new URL(`./preview-${i+1}.png`,import.meta.url),new Uint8Array(await preview.arrayBuffer()));}
console.log(out);

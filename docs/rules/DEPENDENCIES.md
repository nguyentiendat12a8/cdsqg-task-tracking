# Rule thư viện và kiểm chứng

Version cài thực tế theo lockfile/assets; bảng dưới là dependency đã khai báo trong package.json/csproj ngày rà soát, không phải xác nhận phiên bản mới nhất hay tư vấn nâng cấp.

| Nhu cầu | Thư viện / nguồn được dùng |
| --- | --- |
| FE runtime/build | Vue 3, Vite 6; Composition API/script setup |
| Style | Tailwind 4; `frontend/src/style.css` là nơi font/theme chung |
| State toàn ứng dụng | Pinia 2; state form/filter cục bộ dùng ref/computed |
| HTTP | native fetch qua `fetchWithAuth`, `getApiUrl`; tracking store đã dùng apiClient, Axios còn khai báo dependency |
| Tooltip/popover | floating-vue 5 và OverlayPanel |
| Chart | chart.js 4 + vue-chartjs 5; dữ liệu/nhãn dùng adapter feature |
| Toast | vue3-toastify; lỗi field phải có inline text |
| Select/ngày/modal | component hiện có; chưa đưa thêm UI kit |
| Excel | xlsx-js-style 1.2 qua excelRuntime; writer XLSX và reader legacy riêng |
| BE | .NET 9 / ASP.NET Core, EF Core 9.0.2, Npgsql provider 9.0.3 |
| Auth / API docs | JwtBearer 9.0.2, IdentityModel 8.5, Swashbuckle 7.2 |
| Test | xUnit, PostgreSQL integration, Playwright scripts hiện có |

PrimeVue/core 5, themes 4 và xlsx 0.18 đang được khai báo, chưa quyết định sử dụng làm chuẩn control mới. Trước khi gỡ/nâng phải kiểm kê cả src, build, tests và lockfile; không kết luận unused chỉ dựa vào một grep. Không thêm thêm thư viện HTTP/date/icon/UI để giải một màn hình đã có capability tương đương. Compatibility và security review khi nâng package dùng tài liệu chính thức tại thời điểm nâng.

Không import Excel engine vào view eager. Feature được lazy import trước khi tải writer; utility `excelReports.js` có static import cần xét cả import graph. Không đổi writer sang đọc XLS cũ vì có thể mất bảng mã tiếng Việt. Không sửa node_modules; adapter build trong `frontend/build/excelWriter.js` phải kiểm tra và fail khi dependency entry thay đổi. Không tăng chunk warning limit để che bundle lớn.

## Kiểm chứng theo phạm vi

- Tài liệu/rule: kiểm tra link nội bộ, file path và không có credential; không bắt buộc build.
- Chuẩn module: `npm run check:rules --prefix frontend`; formatter/enum thuần: `npm run test:shared --prefix frontend`.
- UI/control: `npm run build --prefix frontend`, browser script phù hợp, desktop/mobile/keyboard; screenshot để review khi đổi layout.
- Query/filter/module/export: `test:refactor`, `test:excel-production` khi tương ứng; verify đầy đủ dữ liệu và tiếng Việt.
- Annual baseline/report: `test:annual` và `ProgressCalculatorTests`.
- Auth/quyền: `SecurityTests`, browser security khi phù hợp; không tự lấy material xác thực từ DB để giả phiên người dùng.
- DB/workflow: `dotnet test backend.Tests/backend.Tests.csproj`; đặt `CDSQG_TEST_POSTGRES` vào DB test riêng để integration không skip; không trỏ DB ứng dụng.

Build/tests không chứng minh mọi UI accessible hoặc mọi endpoint đúng quyền. Báo rõ test nào đã chạy, nào chưa, warning có sẵn và ảnh hưởng. EF InMemory không thay thế PostgreSQL cho SQL/constraint/migration.


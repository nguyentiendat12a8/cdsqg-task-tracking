# Phương án sửa tiến độ cộng dồn — chưa triển khai

## Quy ước số nhập

`Cumulative`: số nhập là phần phát sinh riêng trong kỳ. Ví dụ Q1 = 15, Q2 = 20,
thì lũy kế đến Q2 = 35.

`LatestValue`: số nhập đã là kết quả tính đến thời điểm báo cáo. Ví dụ báo cáo Q2
đã ghi tổng lũy kế 35 thì dùng trực tiếp 35, không cộng lại Q1.

## Cách chọn báo cáo

1. Lọc cùng nhiệm vụ, cùng cơ quan, cùng năm, cùng loại kỳ báo cáo.
2. Chỉ lấy trạng thái Approved; Pending và Rejected không góp vào số chính thức.
3. Với mỗi kỳ, chọn bản Approved mới nhất theo thời điểm gửi báo cáo. Việc duyệt
   một bản cũ sau bản mới không làm bản cũ thay thế bản mới. Cần quy tắc thứ tự
   ổn định nếu thời điểm gửi trùng nhau.
4. Cộng một bản mỗi kỳ trước kỳ đang xem. Kỳ hiện tại lấy bản Approved mới nhất.
   Khi xem trước báo cáo mới, thay giá trị kỳ hiện tại bằng giá trị đang nhập.
5. Quý 0 là báo cáo năm, không được cộng như một quý trước Q1. Không cộng chung
   số liệu tháng, quý và năm. Nếu muốn tổng năm được suy ra từ quý, phải có quy tắc
   ưu tiên riêng để tránh tính cả báo cáo năm lẫn các quý.
6. Các log AgencyId=null cần xác định rõ thuộc cơ quan chủ trì hay là số tổng hợp;
   không tự cộng vào tất cả cơ quan. Nên chuẩn hóa dữ liệu cũ trước khi áp dụng.

## Ví dụ

| Báo cáo | Giá trị | Trạng thái | Đóng góp vào lũy kế Q2 của A |
|---|---:|---|---:|
| A, Q1, bản cũ | 10 | Approved | 0 |
| A, Q1, bản mới | 15 | Approved | 15 |
| A, Q1, bản đang chờ | 18 | Pending | 0 |
| B, Q1 | 100 | Approved | 0 |
| A, Q2 | 20 | Approved | 20 |
| **Tổng A đến Q2** | | | **35** |

## Điểm cần thay đổi trong code

- Tạo một hàm chọn báo cáo hiệu lực và tính lũy kế dùng chung cho ghi báo cáo,
  duyệt/từ chối, PlanningService, dashboard và xuất báo cáo.
- ProgressLog.QuantitativeValue giữ nguyên số phát sinh người dùng nhập. Tách rõ
  số phát sinh, số lũy kế và tỷ lệ hoàn thành trong DTO để giao diện không nhầm.
- Dùng baseline đúng kỳ để tính tỷ lệ; cơ chế fallback baseline hiện tại cũng cần
  sửa để cùng một số lũy kế không cho ra tỷ lệ sai.
- Khi gửi Pending: chỉ tính số xem trước; giữ nguyên số chính thức.
- Khi duyệt/từ chối hoặc sửa kỳ trước: tính lại kết quả chính thức của các kỳ sau
  và AgencyTaskExecution trong cùng transaction. Bản kỳ trước không được trực tiếp
  ghi đè trạng thái tổng hợp của kỳ mới hơn.
- Chạy tác vụ tính lại dữ liệu cũ một lần, trước hết xuất danh sách thay đổi để đối
  chiếu. Không xóa lịch sử báo cáo.

## Kiểm thử bắt buộc

- Hai bản Approved cùng Q1 chỉ góp bản mới nhất.
- Pending/Rejected, cơ quan khác và năm khác không được cộng.
- Q1 không cộng báo cáo năm (Quarter=0).
- Duyệt báo cáo Q1 sau khi đã có Q2 cập nhật lũy kế Q2 nhưng không đổi kỳ hiện tại
  về Q1.
- Báo cáo mới đang Pending không thay đổi số chính thức.
- LatestValue không cộng số của kỳ trước.
- Dashboard, chi tiết nhiệm vụ và Excel hiển thị cùng kết quả.

// Shared Fixed Sections (Mục) & Groups (Nhóm) for Decision 1266 / Phụ lục I & II

export const GOAL_SECTIONS = [
  { value: 'Mục A', label: 'Mục A: MỤC TIÊU ĐẾN NĂM 2030' },
  { value: 'Mục B', label: 'Mục B: MỤC TIÊU ĐẾN NĂM 2045' }
];

export const GOAL_GROUPS = [
  { value: 'Nhóm I: Phát triển Chính phủ số, nâng cao hiệu quả, hiệu lực hoạt động', section: 'Mục A', label: 'Nhóm I: Phát triển Chính phủ số, nâng cao hiệu quả, hiệu lực hoạt động' },
  { value: 'Nhóm II: Phát triển Kinh tế số, nâng cao năng lực cạnh tranh của nền kinh tế', section: 'Mục A', label: 'Nhóm II: Phát triển Kinh tế số, nâng cao năng lực cạnh tranh của nền kinh tế' },
  { value: 'Nhóm III: Phát triển Xã hội số, thu hẹp khoảng cách số, nâng cao chất lượng cuộc sống', section: 'Mục A', label: 'Nhóm III: Phát triển Xã hội số, thu hẹp khoảng cách số, nâng cao chất lượng cuộc sống' },
  { value: 'Nhóm I (2045): Chuyển đổi số toàn diện và bền vững', section: 'Mục B', label: 'Nhóm I: Chuyển đổi số toàn diện và bền vững' }
];

export const TASK_SECTIONS = [];

export const TASK_GROUPS = [
  { value: 'Nhóm I: Hoàn thiện thể chế số', label: 'Nhóm I: Hoàn thiện thể chế số' },
  { value: 'Nhóm II: Phát triển hạ tầng số', label: 'Nhóm II: Phát triển hạ tầng số' },
  { value: 'Nhóm III: Phát triển dữ liệu số, nền tảng số dùng chung', label: 'Nhóm III: Phát triển dữ liệu số, nền tảng số dùng chung' },
  { value: 'Nhóm IV: Phát triển nhân lực số', label: 'Nhóm IV: Phát triển nhân lực số' },
  { value: 'Nhóm V: Phát triển doanh nghiệp công nghệ số', label: 'Nhóm V: Phát triển doanh nghiệp công nghệ số' },
  { value: 'Nhóm VI: Tăng cường hợp tác quốc tế', label: 'Nhóm VI: Tăng cường hợp tác quốc tế' },
  { value: 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm', label: 'Nhóm Thường Xuyên: Công tác truyền thông, chỉ đạo điều hành hàng năm' }
];

export const YEAR_LIST = [
  { value: 2026, label: 'Năm 2026' },
  { value: 2027, label: 'Năm 2027' },
  { value: 2028, label: 'Năm 2028' },
  { value: 2029, label: 'Năm 2029' },
  { value: 2030, label: 'Năm 2030' }
];

export const QUALITATIVE_STATUS_OPTIONS = [
  { value: 'NotStarted', label: 'Chưa thực hiện' },
  { value: 'Drafting', label: 'Đang xây dựng / Soạn thảo' },
  { value: 'Reviewing', label: 'Đang xin ý kiến / Thẩm định' },
  { value: 'Completed', label: 'Đã hoàn thành / Ban hành' }
];

export const QUALITATIVE_STATUS_MAP = {
  'NotStarted': 'Chưa thực hiện',
  '1': 'Chưa thực hiện',
  'Drafting': 'Đang xây dựng / Soạn thảo',
  '2': 'Đang xây dựng / Soạn thảo',
  'Reviewing': 'Đang xin ý kiến / Thẩm định',
  '3': 'Đang xin ý kiến / Thẩm định',
  'Submitted': 'Đang xin ý kiến / Thẩm định',
  'Completed': 'Đã hoàn thành / Ban hành',
  '4': 'Đã hoàn thành / Ban hành'
};

export function getQualitativeStatusLabel(status) {
  if (!status) return 'Chưa thực hiện';
  return QUALITATIVE_STATUS_MAP[status] || status;
}


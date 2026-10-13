// Danh mục lưu trữ gồm văn bản QPPL, hợp nhất và hành chính. Loại không quyết định tính QPPL.
export const LEGAL_DOCUMENT_TYPES = ["Hiến pháp", "Bộ luật", "Luật", "Pháp lệnh", "Lệnh", "Nghị quyết", "Nghị quyết liên tịch", "Nghị định", "Thông tư", "Thông tư liên tịch", "Quyết định", "Văn bản hợp nhất", "Chỉ thị", "Công văn", "Công điện", "Thông báo", "Kế hoạch", "Chương trình", "Đề án", "Báo cáo", "Quy chế", "Quy định", "Hướng dẫn", "Tờ trình", "Khác"];
export const legalDocumentTypeOptions = LEGAL_DOCUMENT_TYPES.map(value => ({ value, label: value }));

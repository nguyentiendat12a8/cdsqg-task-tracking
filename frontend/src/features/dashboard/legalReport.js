import { fetchWithAuth } from '../../services/auth';
import { toast } from 'vue3-toastify';
import { loadExcelWriter } from '../../utils/excelRuntime';
import { styleWorksheet } from '../../utils/excelExport';
import { getApiUrl } from '../../config/api';
import { authState } from '../../services/auth';

export async function exportLegalReport({ formatDate, legalStats, legalFilterDocumentType, legalFilterIssuedFromDate, legalFilterIssuedToDate, legalFilterIssuingAgencyId, legalFilterDraftingAgencyId, legalMinistriesStats, legalProvincesStats, isExportingLegalExcel }) {
  const XLSX = await loadExcelWriter();
  isExportingLegalExcel.value = true;
  toast.info("Đang khởi tạo báo cáo Excel Thống kê Văn bản QPPL...", { autoClose: 2000 });

  try {
    if (!legalStats.value) {
      toast.warning("Chưa có dữ liệu thống kê văn bản QPPL!");
      isExportingLegalExcel.value = false;
      return;
    }

    const wb = XLSX.utils.book_new();
    const now = new Date();
    const timeStr = `${now.toLocaleDateString('vi-VN')} ${now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
    const docTypes = Object.keys(legalStats.value.categoryCounts || {});

    // ==========================================
    // SHEET 1: TỔNG HỢP THỐNG KÊ VB QPPL
    // ==========================================
    const summaryRows = [
      ["BÁO CÁO THỐNG KÊ VĂN BẢN QUY PHẠM PHÁP LUẬT (VB QPPL)"],
      [`Thời gian xuất: ${timeStr} | Tổng số văn bản: ${legalStats.value.totalCount || 0}`],
      [],
      ["CHỈ SỐ TỔNG QUAN THEO LOẠI VĂN BẢN"],
      ["Tổng số văn bản quy phạm pháp luật", legalStats.value.totalCount || 0]
    ];

    docTypes.forEach(t => {
      summaryRows.push([`Văn bản ${t}`, legalStats.value.categoryCounts[t] || 0]);
    });

    const merges = [
      { s: { r: 0, c: 0 }, e: { r: 0, c: Math.max(docTypes.length + 2, 5) } },
      { s: { r: 1, c: 0 }, e: { r: 1, c: Math.max(docTypes.length + 2, 5) } }
    ];

    // Table 1: Ministries Stats (Khối Bộ / Ngành)
    const minList = legalMinistriesStats.value || [];
    if (minList.length > 0) {
      summaryRows.push([]);
      const minRowIdx = summaryRows.length;
      merges.push({ s: { r: minRowIdx, c: 0 }, e: { r: minRowIdx, c: docTypes.length + 2 } });
      summaryRows.push([`THỐNG KÊ SỐ LƯỢNG VĂN BẢN THEO BỘ / NGÀNH TRUNG ƯƠNG (${minList.length} cơ quan)`]);

      const minHeaders = ["STT", "Cơ Quan Ban Hành / Soạn Thảo", "Tổng Số Văn Bản", ...docTypes];
      summaryRows.push(minHeaders);

      minList.forEach((ag, idx) => {
        const row = [idx + 1, ag.agencyName, ag.totalCount || 0];
        docTypes.forEach(t => {
          row.push(ag.typeCounts?.[t] || 0);
        });
        summaryRows.push(row);
      });
    }

    // Table 2: Provinces Stats (Khối Địa Phương)
    const provList = legalProvincesStats.value || [];
    if (provList.length > 0) {
      summaryRows.push([]);
      const provRowIdx = summaryRows.length;
      merges.push({ s: { r: provRowIdx, c: 0 }, e: { r: provRowIdx, c: docTypes.length + 2 } });
      summaryRows.push([`THỐNG KÊ SỐ LƯỢNG VĂN BẢN THEO ĐỊA PHƯƠNG / TỈNH THÀNH (${provList.length} cơ quan)`]);

      const provHeaders = ["STT", "Cơ Quan Ban Hành / Soạn Thảo", "Tổng Số Văn Bản", ...docTypes];
      summaryRows.push(provHeaders);

      provList.forEach((ag, idx) => {
        const row = [idx + 1, ag.agencyName, ag.totalCount || 0];
        docTypes.forEach(t => {
          row.push(ag.typeCounts?.[t] || 0);
        });
        summaryRows.push(row);
      });
    }

    // Table 3: Recent Documents (Level 3 or fallback)
    if (legalStats.value.recentDocuments && legalStats.value.recentDocuments.length > 0) {
      summaryRows.push([]);
      const recentRowIdx = summaryRows.length;
      merges.push({ s: { r: recentRowIdx, c: 0 }, e: { r: recentRowIdx, c: docTypes.length + 2 } });
      summaryRows.push([`DANH SÁCH VĂN BẢN QPPL BAN HÀNH GẦN ĐÂY (${legalStats.value.recentDocuments.length} văn bản)`]);

      summaryRows.push(["STT", "Số Ký Hiệu", "Trích Yếu Nội Dung", "Loại VB", "Cơ Quan Ban Hành", "Ngày Ban Hành", "Trạng Thái"]);
      legalStats.value.recentDocuments.forEach((doc, idx) => {
        summaryRows.push([
          idx + 1,
          doc.documentNumber || doc.code || '—',
          doc.title || '—',
          doc.documentType || '—',
          doc.issuingAgencyName || '—',
          formatDate(doc.issuedDate),
          doc.effectStatus || '—'
        ]);
      });
    }

    const wsSummary = XLSX.utils.aoa_to_sheet(summaryRows);
    wsSummary['!merges'] = merges;
    styleWorksheet(wsSummary, { numCols: Math.max(docTypes.length + 3, 6), headerRowIndex: 5, titleRowIndex: 0 }, XLSX);
    XLSX.utils.book_append_sheet(wb, wsSummary, "Thống Kê Tổng Hợp");

    // ==========================================
    // SHEET 2: DANH SÁCH CHI TIẾT VĂN BẢN QPPL
    // ==========================================
    try {
      const docParams = new URLSearchParams({
        pageNumber: '1',
        pageSize: '1000'
      });

      if (legalFilterDocumentType.value) docParams.append('documentType', legalFilterDocumentType.value);
      if (legalFilterIssuedFromDate.value) docParams.append('fromDate', legalFilterIssuedFromDate.value);
      if (legalFilterIssuedToDate.value) docParams.append('toDate', legalFilterIssuedToDate.value);
      if (legalFilterIssuingAgencyId.value) docParams.append('issuingAgencyId', legalFilterIssuingAgencyId.value);
      if (legalFilterDraftingAgencyId.value) docParams.append('draftingAgencyId', legalFilterDraftingAgencyId.value);

      const userRole = authState.user.value?.role || (authState.isAdmin.value ? 'Admin' : 'Level2');
      if (userRole) docParams.append('userRole', userRole);
      if (authState.user.value?.agencyId) docParams.append('userAgencyId', authState.user.value.agencyId);

      const docRes = await fetchWithAuth(getApiUrl(`/api/legaldocuments?${docParams.toString()}`));
      if (docRes.ok) {
        const docData = await docRes.json();
        const docItems = docData.items || (Array.isArray(docData) ? docData : []);

        if (docItems.length > 0) {
          const detailRows = [
            ["DANH SÁCH VĂN BẢN QUY PHẠM PHÁP LUẬT CHI TIẾT"],
            [`Thời gian xuất: ${timeStr} | Tổng số: ${docItems.length} văn bản`],
            [],
            [
              "STT",
              "Số Ký Hiệu",
              "Trích Yếu Nội Dung",
              "Loại VB",
              "Cơ Quan Ban Hành",
              "Cơ Quan Dự Thảo",
              "Người Ký & Chức Danh",
              "Ngày Ban Hành",
              "Ngày Hiệu Lực",
              "Trạng Thái Hiệu Lực",
              "Lĩnh Vực",
              "Ghi Chú"
            ]
          ];

          docItems.forEach((d, idx) => {
            const signer = d.signerName ? `${d.signerName}${d.signerTitle ? ' (' + d.signerTitle + ')' : ''}` : '—';
            detailRows.push([
              idx + 1,
              d.code || '—',
              d.title || '—',
              d.documentType || '—',
              d.issuingAgencyName || '—',
              d.draftingAgencyName || '—',
              signer,
              formatDate(d.issuedDate),
              formatDate(d.effectiveDate),
              d.effectStatus || '—',
              d.field || '—',
              d.notes || '—'
            ]);
          });

          const wsDetail = XLSX.utils.aoa_to_sheet(detailRows);
          wsDetail['!merges'] = [
            { s: { r: 0, c: 0 }, e: { r: 0, c: 11 } },
            { s: { r: 1, c: 0 }, e: { r: 1, c: 11 } }
          ];

          wsDetail['!cols'] = [
            { wch: 6 },  // STT
            { wch: 18 }, // Số Ký Hiệu
            { wch: 45 }, // Trích Yếu Nội Dung
            { wch: 15 }, // Loại VB
            { wch: 25 }, // Cơ Quan Ban Hành
            { wch: 25 }, // Cơ Quan Dự Thảo
            { wch: 22 }, // Người Ký
            { wch: 15 }, // Ngày Ban Hành
            { wch: 15 }, // Ngày Hiệu Lực
            { wch: 18 }, // Trạng Thái Hiệu Lực
            { wch: 18 }, // Lĩnh Vực
            { wch: 30 }  // Ghi Chú
          ];

          styleWorksheet(wsDetail, { numCols: 12, headerRowIndex: 3, titleRowIndex: 0 }, XLSX);
          XLSX.utils.book_append_sheet(wb, wsDetail, "Danh Sách Văn Bản");
        }
      }
    } catch (e) {
      console.warn("Không thể tải danh sách chi tiết văn bản để xuất Sheet 2:", e);
    }

    const dateFileStr = now.toISOString().slice(0, 10);
    XLSX.writeFile(wb, `Bao_Cao_Thong_Ke_VB_QPPL_${dateFileStr}.xlsx`);
    toast.success("Đã xuất báo cáo Excel Thống kê Văn bản QPPL (gồm 2 Sheet) thành công!");
  } catch (err) {
    console.error("Lỗi khi xuất file Excel thống kê VB QPPL:", err);
    toast.error("Lỗi khi xuất file Excel: " + (err.message || err));
  } finally {
    isExportingLegalExcel.value = false;
  }
}

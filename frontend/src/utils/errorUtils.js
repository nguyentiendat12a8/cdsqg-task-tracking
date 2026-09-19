/**
 * Converts API error responses (including ASP.NET Core ValidationProblemDetails and custom error objects)
 * into human-readable Vietnamese error messages.
 */
export function parseApiError(err, fallbackMessage = 'Có lỗi xảy ra, vui lòng thử lại.') {
  if (!err) return fallbackMessage;

  if (typeof err === 'string') return err;

  // Custom error message from backend
  if (err.error && typeof err.error === 'string') {
    return translateEnglishMessage(err.error);
  }

  // Standard message field
  if (err.message && typeof err.message === 'string' && err.message !== 'One or more validation errors occurred.') {
    return translateEnglishMessage(err.message);
  }

  // ASP.NET Core ValidationProblemDetails dictionary (err.errors)
  if (err.errors && typeof err.errors === 'object') {
    const fieldErrorMessages = [];
    for (const fieldName in err.errors) {
      const messages = err.errors[fieldName];
      if (Array.isArray(messages) && messages.length > 0) {
        messages.forEach(msg => {
          fieldErrorMessages.push(formatFieldError(fieldName, msg));
        });
      }
    }
    if (fieldErrorMessages.length > 0) {
      return fieldErrorMessages.join('. ');
    }
  }

  // ASP.NET Title field fallback
  if (err.title && typeof err.title === 'string') {
    if (err.title === 'One or more validation errors occurred.') {
      return 'Dữ liệu nhập vào chưa đầy đủ hoặc không hợp lệ. Vui lòng kiểm tra lại các trường thông tin.';
    }
    return translateEnglishMessage(err.title);
  }

  return fallbackMessage;
}

function formatFieldError(fieldName, originalMsg) {
  if (!fieldName) return translateEnglishMessage(originalMsg);
  
  const fieldLabel = getFieldLabel(fieldName);
  const lowerMsg = originalMsg.toLowerCase();

  if (lowerMsg.includes('is required')) {
    return `${fieldLabel} không được để trống`;
  }
  if (lowerMsg.includes('invalid')) {
    return `${fieldLabel} không hợp lệ`;
  }

  return `${fieldLabel}: ${translateEnglishMessage(originalMsg)}`;
}

function getFieldLabel(fieldName) {
  if (!fieldName) return 'Trường dữ liệu';
  
  // Handle array property names like Deliverables[0].Title
  const matchArray = fieldName.match(/Deliverables\[(\d+)\]\.(.+)/i);
  if (matchArray) {
    const idx = parseInt(matchArray[1]) + 1;
    const prop = matchArray[2].toLowerCase();
    if (prop === 'title') return `Sản phẩm đầu ra #${idx} (Tên sản phẩm/văn bản)`;
    if (prop === 'duedate') return `Sản phẩm đầu ra #${idx} (Hạn chót)`;
    return `Sản phẩm đầu ra #${idx}`;
  }

  const name = fieldName.toLowerCase();
  if (name.includes('title')) return 'Tên mục tiêu / nhiệm vụ';
  if (name.includes('leadagency')) return 'Đơn vị chủ trì';
  if (name.includes('coordinatingagency')) return 'Cơ quan phối hợp';
  if (name.includes('startdate')) return 'Ngày bắt đầu';
  if (name.includes('duedate')) return 'Ngày hoàn thành';
  if (name.includes('deliverables')) return 'Danh mục sản phẩm đầu ra';
  if (name.includes('code')) return 'Mã mục tiêu / nhiệm vụ';
  if (name.includes('section')) return 'Mục';
  if (name.includes('group')) return 'Nhóm trọng tâm';

  return `Trường '${fieldName}'`;
}

function translateEnglishMessage(msg) {
  if (!msg) return '';
  if (msg === 'One or more validation errors occurred.') {
    return 'Dữ liệu nhập vào chưa đầy đủ hoặc không hợp lệ. Vui lòng kiểm tra các trường thông tin.';
  }
  if (msg.includes('The Title field is required')) {
    return 'Tên mục tiêu / nhiệm vụ không được để trống.';
  }
  if (msg.includes('The LeadAgencyId field is required')) {
    return 'Vui lòng chọn đơn vị chủ trì.';
  }
  return msg;
}

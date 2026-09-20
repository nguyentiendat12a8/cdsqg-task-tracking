/**
 * Converts API error responses into clean, concise, human-readable Vietnamese messages.
 */
export function parseApiError(err, fallbackMessage = 'Lưu thất bại.') {
  if (!err) return fallbackMessage;

  if (typeof err === 'string') {
    return sanitizeErrorMessage(err, fallbackMessage);
  }

  // Custom error message from backend
  if (err.error && typeof err.error === 'string') {
    return sanitizeErrorMessage(err.error, fallbackMessage);
  }

  // Standard message field
  if (err.message && typeof err.message === 'string' && err.message !== 'One or more validation errors occurred.') {
    return sanitizeErrorMessage(err.message, fallbackMessage);
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
      return 'Dữ liệu nhập vào chưa đầy đủ hoặc không hợp lệ. Vui lòng kiểm tra lại.';
    }
    return sanitizeErrorMessage(err.title, fallbackMessage);
  }

  return fallbackMessage;
}

function sanitizeErrorMessage(rawMsg, fallbackMessage) {
  if (!rawMsg || typeof rawMsg !== 'string') return fallbackMessage;

  const lower = rawMsg.toLowerCase();
  
  // Intercept any technical / stack trace / raw database error strings
  if (
    lower.includes('database') ||
    lower.includes('expected to affect') ||
    lower.includes('exception') ||
    lower.includes('sql') ||
    lower.includes('ef core') ||
    lower.includes('concurrency') ||
    lower.includes('internal server error') ||
    lower.includes('system.') ||
    lower.includes('linkid=') ||
    lower.includes('failed to fetch') ||
    lower.includes('networkerror')
  ) {
    return fallbackMessage || 'Có lỗi xảy ra, vui lòng thử lại.';
  }

  return translateEnglishMessage(rawMsg, fallbackMessage);
}

function formatFieldError(fieldName, originalMsg) {
  if (!fieldName) return translateEnglishMessage(originalMsg, 'Trường dữ liệu không hợp lệ.');
  
  const fieldLabel = getFieldLabel(fieldName);
  const lowerMsg = originalMsg.toLowerCase();

  if (lowerMsg.includes('is required')) {
    return `${fieldLabel} không được để trống`;
  }
  if (lowerMsg.includes('invalid')) {
    return `${fieldLabel} không hợp lệ`;
  }

  return `${fieldLabel} không hợp lệ`;
}

function getFieldLabel(fieldName) {
  if (!fieldName) return 'Trường dữ liệu';
  
  const matchArray = fieldName.match(/Deliverables\[(\d+)\]\.(.+)/i);
  if (matchArray) {
    const idx = parseInt(matchArray[1]) + 1;
    const prop = matchArray[2].toLowerCase();
    if (prop === 'title') return `Sản phẩm #${idx}`;
    if (prop === 'duedate') return `Hạn chót sản phẩm #${idx}`;
    return `Sản phẩm #${idx}`;
  }

  const name = fieldName.toLowerCase();
  if (name.includes('title')) return 'Tên mục tiêu / nhiệm vụ';
  if (name.includes('leadagency')) return 'Đơn vị chủ trì';
  if (name.includes('coordinatingagency')) return 'Cơ quan phối hợp';
  if (name.includes('startdate')) return 'Ngày bắt đầu';
  if (name.includes('duedate')) return 'Ngày hoàn thành';
  if (name.includes('deliverables')) return 'Danh mục sản phẩm';
  if (name.includes('code')) return 'Mã nhiệm vụ';

  return 'Thông tin nhập vào';
}

function translateEnglishMessage(msg, fallbackMessage = 'Lưu thất bại.') {
  if (!msg) return fallbackMessage;
  if (msg === 'One or more validation errors occurred.') {
    return 'Dữ liệu nhập vào chưa đầy đủ hoặc không hợp lệ.';
  }
  if (msg.includes('The Title field is required')) {
    return 'Tên mục tiêu / nhiệm vụ không được để trống.';
  }
  if (msg.includes('The LeadAgencyId field is required')) {
    return 'Vui lòng chọn đơn vị chủ trì.';
  }
  // If the message is in English (contains mostly ASCII Latin characters without Vietnamese diacritics)
  if (/^[a-zA-Z0-9\s.,!?:;'"()-]+$/.test(msg.trim())) {
    return fallbackMessage;
  }
  return msg;
}

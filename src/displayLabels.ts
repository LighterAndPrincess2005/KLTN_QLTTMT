// Presentation labels only. API identifiers and enum values remain unchanged.
const states:Record<string,string>={CHO_THANH_TOAN:'Chờ thanh toán',DA_XAC_NHAN:'Đã xác nhận',DA_HUY:'Đã hủy',DA_CHUYEN:'Đã chuyển',HET_HAN:'Hết hạn',CHO_CHUYEN:'Chờ chuyển',CO_MAT:'Có mặt',VANG:'Vắng',CHO_THAM_GIA:'Chưa diễn ra',HUY_BO:'Đã hủy',CHO_XU_LY:'Chờ trung tâm xử lý',DA_TRA_LOI:'Trung tâm đã trả lời',CHO_DUYET:'Chờ duyệt',DA_DUYET:'Đã duyệt',DA_THUC_HIEN:'Đã thực hiện',TU_CHOI:'Không được duyệt',DA_DAT:'Đã xếp lịch bù',DA_HOAN_TAT:'Đã học bù',HOAN_THANH:'Hoàn thành',DA_HOAN_THANH:'Đã hoàn thành',DE_XUAT:'Chờ duyệt kết quả',DAT:'Đạt',KHONG_DAT:'Chưa đạt',CHUA_DAT:'Chưa đạt',DA_XEP_LICH:'Đã xếp lịch',DA_TO_CHUC:'Đã hoàn thành',MOI_GUI:'Mới gửi',DA_DOC:'Đã đọc',DANG_XU_LY:'Đang xử lý',DA_DONG:'Đã đóng',CHO_GUI:'Chờ gửi email',DANG_GUI:'Đang gửi email',DA_GUI:'Đã gửi email',GUI_LOI:'Gửi email lỗi',KHONG_CO_EMAIL:'Chưa có email dùng được'};
export const statusLabel=(value:string)=>states[value]||'Đang cập nhật';
export const publicReference=(value:string|undefined|null)=>value&&!/^(MAIN|DEMO|TEST|CRUD)[_-]/i.test(value)?value:null;
const levels:Record<string,string>={JUNIOR:'Junior',FOUNDATION:'Foundation',PRE_BASIC:'Pre-Basic',BASIC:'Basic',INTERMEDIATE:'Intermediate',ACRYLIC:'Acrylic',WATERCOLOR:'Màu nước',CLAY:'Đất nặn'};
export const levelLabel=(value:string)=>levels[value]||(/^[A-Z0-9_]+$/.test(value)?'Hồ sơ học viên':value);
export function apiErrorLabel(value:unknown){
 const text=typeof value==='string'?value:'';
 return /SqlException|Microsoft\.EntityFrameworkCore|Invalid (?:column|object) name|\bdbo\.|\bSELECT\b.+\bFROM\b|\b(?:StackTrace|RowVersion|ConnectionString)\b/i.test(text)?'Chưa xử lý được yêu cầu. Vui lòng thử lại hoặc liên hệ trung tâm.':text;
}

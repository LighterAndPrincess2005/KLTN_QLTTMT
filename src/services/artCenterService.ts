import type { ArtClass, Course, StudentProfile } from '../types';
import { courseImage } from '../data/courseImages';
const base=(import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');
type Session={token:string;expires:number};
let session:Session|null=null;
try{session=JSON.parse(sessionStorage.getItem('artora.session')||'null')}catch{sessionStorage.removeItem('artora.session')}
export class ApiError extends Error { constructor(message:string,public status:number){super(message)} }
export const clearSession=()=>{session=null;sessionStorage.removeItem('artora.session')};
export const signedIn=()=>!!session&&session.expires>Date.now();
export async function request<T>(path:string,body?:unknown,method=body===undefined?'GET':'POST'):Promise<T>{
 const headers:Record<string,string>={'Accept':'application/json'};
 if(body!==undefined)headers['Content-Type']='application/json';
 if(signedIn())headers.Authorization=`Bearer ${session!.token}`;
 let res:Response;
 try{res=await fetch(base+path,{method,headers,body:body===undefined?undefined:JSON.stringify(body),signal:AbortSignal.timeout(20000)})}
 catch{throw new ApiError('Chưa kết nối được trung tâm. Vui lòng thử lại sau.',0)}
 const data=await res.json().catch(()=>null);
 if(!res.ok){if(res.status===401){clearSession();window.dispatchEvent(new Event('artora:session-expired'))}
 throw new ApiError(data?.title||data?.thongBao||data?.error||Object.values(data?.errors||{}).flat().join(' ')||'Không xử lý được yêu cầu.',res.status)}
 return data as T;
}
type Page<T>={duLieu:T[];tongSo:number};
async function all<T>(path:string){const out:T[]=[];for(let trang=1;trang<=10000;trang++){const p=await request<Page<T>>(`${path}?trang=${trang}&kichThuoc=100`);out.push(...p.duLieu);if(out.length>=p.tongSo||!p.duLieu.length)break}return out}
type CourseDto={id:number;loaiKhoaHocId:number;tenKhoa:string;capDo:string;tuoiToiThieu:number|null;yeuCauDauVao:string|null;mucTieu:string;soBuoi:number;soTietMoiBuoi:number;phutMoiTiet:number;hocPhi:number};
type ClassDto={id:number;khoaHocId:number;maLop:string;tenLop:string;ngayKhaiGiangDuKien:string;lichHocDuKien:string;siSoToiDa:number;hocPhiApDung:number;moDangKyLuc:string;dongDangKyLuc:string;soChoDaGiu:number};
export type Member={id:number;tenDangNhap:string;vaiTro:string;thanhVienId:number|null};
export type Enrollment={id:number;maDangKy:string;hoSoTheoHocId:number;hocVienId:number;lopHocId:number;khoaHocId?:number;coTheQuanLy?:boolean;trangThai:string;hanGiuCho:string;hocVien:string;tenKhoa:string;tenLop:string;lichHoc:string;soBuoi:number};
export type Attendance={id:number;buoiHocId:number;ngayHoc:string;ketThuc?:string;thuTu?:number;chuDe?:string;tenPhong?:string;trangThaiBuoi?:string;trangThai:string;loaiThamGia?:string;nhanXet:string|null;diemSanPham?:number|null;sanPhamUrl:string|null;noiDungDuocTinhId:number;baoNghiLuc?:string|null;ghiLuc?:string|null;nguoiNhanXet?:string|null};
export type Makeup={id:number;diemDanhVangId:number;diemDanhBuId:number|null;trangThai:string;hanHoanTat:string;ngayBu:string|null;tenLopBu:string|null};
export type CourseResult={ketLuan:string;nhanXetTongKet:string|null;diemCuoiKhoaChot:number|null;soXacNhan:string|null};
export type Progress={soNoiDungDaThamGia:number;diemDanh:Attendance[];hocBu?:Makeup[];ketQua:CourseResult|null};
export type Feedback={id:number;dangKyId:number;noiDung:string;traLoi:string|null;guiLuc:string;trangThai:string;kind:'PHAN_HOI'|'HOC_BU'|'DOI_LICH';diemDanhId:number|null};
export type Change={id:number;dangKyNguonId:number;loaiYeuCau:string;lyDo:string;trangThai:string;guiLuc:string};
export type TeacherSession={id:number;lopHocId:number;maLop:string;tenLop:string;tenKhoa:string;batDau:string;ketThuc:string;trangThai:string;thuTu:number;chuDe:string;tenPhong:string;vaiTro:string;soHocVien:number};
export type RosterRow={id:number;dangKyId:number;hocVienId:number;hoTen:string;ngaySinh:string;luuY:string|null;loaiThamGia:string;trangThai:string;baoNghiLuc:string|null;phutThamDu:number|null;nhanXet:string|null;diemSanPham:number|null;sanPhamUrl:string|null;rowVersion:string};
const field=(o:Record<string,unknown>,name:string)=>o[name]??o[name[0].toLowerCase()+name.slice(1)];
export const vnTime=(v:string)=>new Date(/(?:Z|[+-]\d\d:\d\d)$/.test(v)?v:v+'+07:00');
export const dateTime=(v:string)=>vnTime(v).toLocaleString('vi-VN');
export const artCenterService={
 async getCourses():Promise<Course[]>{const [rows,types]=await Promise.all([all<CourseDto>('/api/khoa-hoc'),request<{id:number;tenLoai:string}[]>('/api/loai-khoa-hoc')]);return rows.map((c,i)=>({id:String(c.id),title:c.tenKhoa,ageMin:c.tuoiToiThieu||0,level:c.capDo,sessions:c.soBuoi,minutesPerSession:c.soTietMoiBuoi*c.phutMoiTiet,maxStudents:0,tuitionFee:c.hocPhi,materialFee:0,category:types.find(x=>x.id===c.loaiKhoaHocId)?.tenLoai||'Mỹ thuật',color:['#ce6249','#327562','#bd8b34'][i%3],image:courseImage(c.capDo),skills:[c.mucTieu],prerequisites:c.yeuCauDauVao?[c.yeuCauDauVao]:[]}))},
 async getClasses():Promise<ArtClass[]>{return (await all<ClassDto>('/api/lop-hoc')).map(c=>{const now=Date.now();return{id:String(c.id),code:c.maLop,title:c.tenLop,courseId:String(c.khoaHocId),schedule:c.lichHocDuKien,startDateISO:c.ngayKhaiGiangDuKien,startDate:new Date(c.ngayKhaiGiangDuKien+'T00:00:00+07:00').toLocaleDateString('vi-VN'),registrationDeadline:dateTime(c.dongDangKyLuc),deadline:c.dongDangKyLuc,teacher:'',room:'',capacity:c.siSoToiDa,enrolled:c.soChoDaGiu,tuitionFee:c.hocPhiApDung,mode:'Trực tiếp',status:now<vnTime(c.moDangKyLuc).getTime()?'Chưa mở đăng ký':now>=vnTime(c.dongDangKyLuc).getTime()?'Đã khóa đăng ký':c.soChoDaGiu>=c.siSoToiDa?'Đủ chỗ':'Đang nhận đăng ký'}})},
 async login(name:string,password:string){const x=await request<{accessToken:string;expiresIn:number}>('/api/tai-khoan/dang-nhap',{TenDangNhap:name,MatKhau:password});session={token:x.accessToken,expires:Date.now()+x.expiresIn*1000};sessionStorage.setItem('artora.session',JSON.stringify(session));return this.me()},
 signup:(body:unknown)=>request('/api/tai-khoan/dang-ky',body),
 me:()=>request<Member>('/api/tai-khoan/toi'),
 async logout(){try{await request('/api/tai-khoan/dang-xuat',{})}finally{clearSession()}},
 async getStudentProfiles():Promise<StudentProfile[]>{const rows=await request<{hocVienId:number;hoTen:string;ngaySinh:string;capDoDeXuat:string|null;trangThai:string;xacNhanLuc:string|null;canXoa?:boolean;quyenDaiDien:string}[]>('/api/hoc-vien/toi');return rows.map(s=>({id:String(s.hocVienId),name:s.hoTen,birthDate:s.ngaySinh,level:s.capDoDeXuat||'Chưa đánh giá',completedCourseIds:[],existingSchedules:[],verified:!!s.xacNhanLuc&&s.trangThai==='HOAT_DONG',canDelete:s.canXoa===true,canRegister:!!s.xacNhanLuc&&s.trangThai==='HOAT_DONG'&&['DAY_DU','DANG_KY'].includes(s.quyenDaiDien)}))},
 addStudent:(body:unknown)=>request('/api/hoc-vien/toi',body),
 deleteStudent:(id:string)=>request(`/api/hoc-vien/toi/${encodeURIComponent(id)}`,undefined,'DELETE'),
 submitRegistration:(body:unknown)=>request<Record<string,unknown>>('/api/dang-ky',body),
 enrollments:()=>request<Enrollment[]>('/api/dang-ky/toi'),
 async progress(id:number):Promise<Progress>{const data=await request<Omit<Progress,'ketQua'>&{ketQua:Record<string,unknown>|null}>(`/api/hoc-tap/ho-so/${id}/tien-do`);const result=data.ketQua;return {...data,ketQua:result?{ketLuan:String(field(result,'KetLuan')||''),nhanXetTongKet:field(result,'NhanXetTongKet') as string|null,diemCuoiKhoaChot:field(result,'DiemCuoiKhoaChot') as number|null,soXacNhan:field(result,'SoXacNhan') as string|null}:null}},
 absence:(id:number,reason:string)=>request(`/api/hoc-tap/diem-danh/${id}/bao-nghi`,{LyDo:reason}),
 feedback:(id:number,text:string)=>request('/api/phan-hoi',{DangKyId:id,NoiDung:text}),
 async feedbackHistory():Promise<Feedback[]>{const rows=await request<Record<string,unknown>[]>('/api/phan-hoi');return rows.map(row=>{const raw=String(field(row,'NoiDung')||''),match=raw.match(/^\[(HOC_BU|DOI_LICH):(\d+)\]\n/);return {id:Number(field(row,'Id')),dangKyId:Number(field(row,'DangKyId')),noiDung:raw.replace(/^\[(HOC_BU|DOI_LICH):\d+\]\n/,''),traLoi:field(row,'TraLoi') as string|null,guiLuc:String(field(row,'GuiLuc')),trangThai:String(field(row,'TrangThai')),kind:match?match[1] as 'HOC_BU'|'DOI_LICH':'PHAN_HOI',diemDanhId:match?Number(match[2]):null}})},
 async changes():Promise<Change[]>{return (await request<Record<string,unknown>[]>('/api/thay-doi')).map(row=>({id:Number(field(row,'Id')),dangKyNguonId:Number(field(row,'DangKyNguonId')),loaiYeuCau:String(field(row,'LoaiYeuCau')),lyDo:String(field(row,'LyDo')||''),trangThai:String(field(row,'TrangThai')),guiLuc:String(field(row,'GuiLuc'))}))},
 learningRequest:(id:number,kind:'HOC_BU'|'DOI_LICH',reason:string,preferred:string)=>request('/api/hoc-tap/de-nghi',{DiemDanhId:id,LoaiYeuCau:kind,LyDo:reason,KhungGioMongMuon:preferred.trim()||null}),
 change:(id:number,text:string,key:string)=>request('/api/thay-doi',{DangKyId:id,LoaiYeuCau:'HUY',NguyenNhan:'NGUOI_HOC',LyDo:text,IdempotencyKey:key}),
 transfer:(id:number,target:number,text:string,key:string)=>request('/api/thay-doi',{DangKyId:id,LopDichId:target,LoaiYeuCau:'CHUYEN',NguyenNhan:'NGUOI_HOC',LyDo:text,IdempotencyKey:key}),
 teacherSchedule:()=>request<TeacherSession[]>('/api/hoc-tap/giao-vien/buoi-cua-toi'),
 teacherRoster:(id:number)=>request<RosterRow[]>(`/api/hoc-tap/buoi/${id}/danh-sach`),
 recordAttendance:(id:number,body:unknown)=>request(`/api/hoc-tap/diem-danh/${id}`,body,'PUT'),
 closeSession:(id:number)=>request(`/api/hoc-tap/buoi/${id}/hoan-thanh`,{})
};

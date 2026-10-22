import {courses as originals} from '../data/mockData';
import {courseImage} from '../data/courseImages';
import type {ArtClass} from '../types';
import {teacherDemoRequest} from './teacherDemo';
import {demoLearning} from './demoLearning';
export const demoMode=import.meta.env.DEV&&new URLSearchParams(location.search).get('demo')==='1';
export const demoCourses=originals.map((c,i)=>({...c,id:String(i+1),image:courseImage(['JUNIOR','FOUNDATION','PRE_BASIC','BASIC','ACRYLIC','CLAY'][i]),category:i<4?'Chính khóa':'Bổ trợ'}));
const future=(days:number,weekday=6)=>{const date=new Date(new Date().toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'})+'T12:00:00Z');date.setUTCDate(date.getUTCDate()+days);date.setUTCDate(date.getUTCDate()+(weekday-date.getUTCDay()+7)%7);return date.toISOString().slice(0,10)};
export const demoClasses:ArtClass[]=Array.from({length:18},(_,i)=>{const c=demoCourses[i%6],start=future(14+Math.floor(i/6)*28,i%2?0:6);return{id:String(i+1),code:`BUI-${String(i+1).padStart(3,'0')}`,title:c.title+' · '+(i%2?'Chủ nhật':'Thứ Bảy'),courseId:c.id,schedule:(i%2?'Chủ nhật':'Thứ Bảy')+' · 09:00–'+(c.minutesPerSession===90?'10:30':'11:00'),startDateISO:start,endDateISO:new Date(new Date(start+'T12:00:00Z').getTime()+(c.sessions-1)*7*86400000).toISOString().slice(0,10),startDate:new Date(start+'T00:00:00').toLocaleDateString('vi-VN'),registrationDeadline:start+' 08:00',teacher:'Giáo viên bộ môn',room:'Phòng mỹ thuật',capacity:10,enrolled:i===4?10:3,tuitionFee:c.tuitionFee,mode:'Trực tiếp',status:i===4?'Đủ chỗ':i>11?'Chưa mở đăng ký':'Đang nhận đăng ký',promotion:i%3===0?{code:'BUIMOI10',label:'Ưu đãi trình diễn 10%',percent:10}:undefined}});
const read=(key:string,fallback:unknown)=>{try{return JSON.parse(sessionStorage.getItem('bui.demo.'+key)||'null')??fallback}catch{return fallback}};
const save=(key:string,value:unknown)=>sessionStorage.setItem('bui.demo.'+key,JSON.stringify(value));
export function demoRequest(path:string,body?:unknown,method='GET'):unknown{
 const data=(body||{}) as Record<string,unknown>;
 if(path.includes('/dang-nhap')){save('teacherRole',String(data.TenDangNhap).toLowerCase()==='gv_demo');return {accessToken:'demo-local',expiresIn:86400}};
 if(path.includes('/dang-xuat'))return {};
 if(path==='/api/tai-khoan/dang-ky'){save('account',{hoTen:data.HoTen,dienThoai:data.DienThoai,email:data.Email,ngaySinh:data.NgaySinh});return {}};
 if(path==='/api/tai-khoan/toi'&&method==='PUT'){
  if(read('teacherRole',false))throw new Error('Tài khoản không liên kết thành viên.');
  const current=read('account',{});if(String(current.rowVersion||'1')!==data.RowVersion)throw new Error('Thông tin đã thay đổi. Vui lòng mở lại.');
  save('account',{...current,hoTen:data.HoTen,dienThoai:data.DienThoai,email:data.Email,ngaySinh:data.NgaySinh,rowVersion:String(Number(current.rowVersion||1)+1)});return {};
 }
 if(path.startsWith('/api/hoc-vien/toi/')&&method==='PUT'){
  const students=read('students',[]),student=students.find((s:{hocVienId:number})=>String(s.hocVienId)===path.split('/').pop());
  if(!student||student.quyenDaiDien!=='DAY_DU')throw new Error('Không có quyền sửa hồ sơ.');
  if(String(student.rowVersion||'1')!==data.RowVersion)throw new Error('Hồ sơ đã thay đổi. Vui lòng mở lại.');
  const enrollments=read('enrollments',[]);for(const row of enrollments.filter((r:{hocVienId:number;trangThai:string})=>r.hocVienId===student.hocVienId&&r.trangThai==='DA_XAC_NHAN')){const cl=demoClasses.find(c=>Number(c.id)===row.lopHocId),course=demoCourses.find(c=>c.id===cl?.courseId);if(cl&&course){const birth=new Date(String(data.NgaySinh)+'T12:00:00Z'),at=new Date(cl.startDateISO+'T12:00:00Z');const age=at.getUTCFullYear()-birth.getUTCFullYear()-(at.getUTCMonth()<birth.getUTCMonth()||at.getUTCMonth()===birth.getUTCMonth()&&at.getUTCDate()<birth.getUTCDate()?1:0);if(age<course.ageMin||age<18&&student.quanHe==='TU_BAN_THAN')throw new Error('Ngày sinh mới không đáp ứng điều kiện tuổi của lớp đã đăng ký.');}}
  Object.assign(student,{hoTen:data.HoTen,ngaySinh:data.NgaySinh,lienHeKhanCapTen:data.LienHeKhanCapTen,lienHeKhanCapSDT:data.LienHeKhanCapSDT,mucTieuHoc:data.MucTieuHoc,rowVersion:String(Number(student.rowVersion||1)+1)});save('students',students);return {};
 }
 if(path==='/api/tai-khoan/toi'&&read('teacherRole',false))return {id:901,tenDangNhap:'gv_demo',vaiTro:'GIAO_VIEN',thanhVienId:null,hoTen:'Giáo viên trình diễn'};
 if(path.startsWith('/api/hoc-tap/')&&read('teacherRole',false)){const result=teacherDemoRequest(path,data,method);if(result!==undefined)return result}
 if(path==='/api/tai-khoan/toi')return {id:1,tenDangNhap:'trinh_dien',vaiTro:'THANH_VIEN',thanhVienId:1,hoTen:'Nguyễn Minh Anh',dienThoai:'0901234567',ngaySinh:'1992-05-12',rowVersion:'1',...read('account',{})};
 if(path==='/api/hoc-vien/toi'){
 const students=read('students',[{hocVienId:1,hoTen:'Nguyễn Bảo An',ngaySinh:'2018-06-18',capDoDeXuat:'Nền tảng',trangThai:'HOAT_DONG',xacNhanLuc:new Date().toISOString(),quyenDaiDien:'DAY_DU',canXoa:true,canSua:true,rowVersion:'1',lienHeKhanCapTen:'Nguyễn Minh Anh',lienHeKhanCapSDT:'0901234567'}]);save('students',students);
 if(method==='POST'){const student={quanHe:data.QuanHe,canSua:true,rowVersion:'1',lienHeKhanCapTen:data.LienHeKhanCapTen,lienHeKhanCapSDT:data.LienHeKhanCapSDT,hocVienId:Date.now(),hoTen:data.HoTen,ngaySinh:data.NgaySinh,capDoDeXuat:'Tự chọn',trangThai:'HOAT_DONG',xacNhanLuc:new Date().toISOString(),quyenDaiDien:'DAY_DU',canXoa:true};students.push(student);save('students',students);return {id:student.hocVienId,...student}}return students.map((s:Record<string,unknown>)=>({...s,canSua:s.quyenDaiDien==='DAY_DU',rowVersion:s.rowVersion||'1'}));
 }
 if(path.startsWith('/api/hoc-vien/toi/')&&method==='DELETE'){save('students',read('students',[]).filter((s:{hocVienId:number})=>String(s.hocVienId)!==path.split('/').pop()));return {}};
 if(path==='/api/dang-ky'&&method==='POST'){
 const cl=demoClasses.find(c=>c.id===String(data.LopHocId));if(!cl||cl.status!=='Đang nhận đăng ký'||cl.enrolled>=cl.capacity)throw new Error('Lớp không còn nhận đăng ký.');
 const rows=read('enrollments',[]);if(rows.some((r:{hocVienId:number;lopHocId:number})=>r.hocVienId===data.HocVienId&&r.lopHocId===data.LopHocId))throw new Error('Học viên đã đăng ký lớp này.');
 const student=read('students',[]).find((s:{hocVienId:number})=>s.hocVienId===data.HocVienId);const id=Date.now();const row={id,maDangKy:'BUI-'+String(id).slice(-6),hoSoTheoHocId:id,hocVienId:data.HocVienId,lopHocId:data.LopHocId,khoaHocId:Number(cl.courseId),coTheQuanLy:true,trangThai:'DA_XAC_NHAN',hanGiuCho:new Date(Date.now()+48*3600000).toISOString(),hocVien:student?.hoTen||'Học viên',tenKhoa:demoCourses.find(c=>c.id===cl.courseId)?.title,tenLop:cl.title,lichHoc:cl.schedule,soBuoi:16};rows.push(row);save('enrollments',rows);cl.enrolled++;return row;
 }
 if(path==='/api/dang-ky/toi')return read('enrollments',[]);
 if(path.endsWith('/tien-do'))return demoLearning(Number(path.split('/')[4]),read('enrollments',[]),demoClasses,demoCourses);
 if(path==='/api/phan-hoi'||path==='/api/thay-doi')return [];
 if(path.endsWith('/noi-dung')){const c=demoCourses.find(c=>c.id===path.split('/')[3]);return Array.from({length:c?.sessions||8},(_,i)=>({id:i+1,thuTu:i+1,chuDe:['Làm quen chất liệu','Nét và hình','Bố cục và hòa sắc','Hoàn thiện tác phẩm'][i%4],noiDung:'Quan sát mẫu, thực hành kỹ thuật và hoàn thiện bài vẽ theo hướng dẫn.',soTiet:(c?.minutesPerSession||120)/30,yeuCauSanPham:'Một bài thực hành theo chủ đề.'}))};
 throw new Error('Tính năng này cần kết nối API.');
}

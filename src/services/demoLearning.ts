import type {Attendance,Enrollment,Progress} from './artCenterService';
import type {ArtClass,Course} from '../types';
export function demoLearning(profileId:number,enrollments:Enrollment[],classes:ArtClass[],courses:Course[]):Progress{
 const rows:Attendance[]=[];
 for(const enrollment of enrollments.filter(e=>e.hoSoTheoHocId===profileId&&e.trangThai==='DA_XAC_NHAN')){
  const cls=classes.find(c=>Number(c.id)===enrollment.lopHocId),course=courses.find(c=>c.id===cls?.courseId);
  if(!cls?.startDateISO||!course)continue;
  const clock=cls.schedule.match(/(\d{2}:\d{2})/);if(!clock)continue;
  const initial=new Date(cls.startDateISO+'T'+clock[1]+':00+07:00').getTime();
  for(let i=0;i<course.sessions;i++){const begin=new Date(initial+i*7*86400000);rows.push({id:enrollment.id+i,buoiHocId:enrollment.lopHocId*1000+i,dangKyId:enrollment.id,lopHocId:enrollment.lopHocId,tenLop:cls.title,ngayHoc:begin.toISOString(),ketThuc:new Date(begin.getTime()+course.minutesPerSession*60000).toISOString(),thuTu:i+1,chuDe:['Làm quen chất liệu','Nét và hình','Bố cục và hòa sắc','Thực hành sáng tạo'][i%4],tenPhong:cls.room,giaoVien:cls.teacher,trangThaiBuoi:'DA_XEP_LICH',trangThai:'CHO_THAM_GIA',loaiThamGia:'HOC_CHINH',nhanXet:null,sanPhamUrl:null,noiDungDuocTinhId:i+1})}
 }
 return {soNoiDungDaThamGia:0,diemDanh:rows,hocBu:[],ketQua:null};
}

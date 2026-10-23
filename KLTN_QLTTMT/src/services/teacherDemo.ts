import type {RosterRow,TeacherSession} from './artCenterService';
const read=<T,>(key:string,fallback:T):T=>{try{return JSON.parse(sessionStorage.getItem('bui.teacher.'+key)||'null')??fallback}catch{return fallback}};
const save=(key:string,value:unknown)=>sessionStorage.setItem('bui.teacher.'+key,JSON.stringify(value));
export function teacherDemoRequest(path:string,data:Record<string,unknown>,method:string):unknown{
 const key=new Date().toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'});
 const start=new Date(key+'T00:00:00+07:00');
 // Anchor once per day so refreshing never moves a demo session's time.
 const anchor=read<string>('anchor-'+key,new Date(Math.max(start.getTime(),Date.now()-30*60000)).toISOString());save('anchor-'+key,anchor);
 const sessions=read<TeacherSession[]>('sessions-'+key,[{id:901,lopHocId:901,maLop:'DEMO-GV01',tenLop:'Lớp thực hành demo A',tenKhoa:'Màu sắc nền tảng',batDau:anchor,ketThuc:new Date(new Date(anchor).getTime()+90*60000).toISOString(),trangThai:'DA_XEP_LICH',thuTu:3,chuDe:'Quan sát và phối màu',tenPhong:'Phòng Vẽ 01',vaiTro:'CHINH',soHocVien:3},{id:902,lopHocId:902,maLop:'DEMO-GV02',tenLop:'Lớp thực hành demo B',tenKhoa:'Ký họa',batDau:new Date(start.getTime()+86400000+9*3600000).toISOString(),ketThuc:new Date(start.getTime()+86400000+11*3600000).toISOString(),trangThai:'DA_XEP_LICH',thuTu:1,chuDe:'Nét và hình khối',tenPhong:'Phòng Vẽ 02',vaiTro:'CHINH',soHocVien:3}]);
 const roster=(id:number)=>read<RosterRow[]>('roster-'+key+'-'+id,['Nguyễn Bảo An','Trần Minh Khuê','Lê Gia Huy'].map((hoTen,i)=>({id:id*10+i,dangKyId:id*10+i,hocVienId:i+1,hoTen,ngaySinh:'2017-06-18',luuY:null,loaiThamGia:i===2?'HOC_BU':'HOC_CHINH',trangThai:'CHO_THAM_GIA',baoNghiLuc:null,phutThamDu:null,nhanXet:null,diemSanPham:null,sanPhamUrl:null,rowVersion:'0'})));
 if(path==='/api/hoc-tap/giao-vien/buoi-cua-toi')return sessions;
 const match=path.match(/^\/api\/hoc-tap\/buoi\/(\d+)\/(danh-sach|hoan-thanh)$/);
 if(match){const id=Number(match[1]),session=sessions.find(s=>s.id===id);if(!session)throw Error('Không có buổi demo này.');if(match[2]==='danh-sach')return roster(id);const rows=roster(id);if(Date.now()<new Date(session.ketThuc).getTime()||rows.some(r=>r.trangThai==='CHO_THAM_GIA'))throw Error('Buổi chưa kết thúc hoặc chưa điểm danh đầy đủ.');session.trangThai='DA_TO_CHUC';save('sessions-'+key,sessions);return {}}
 const attend=path.match(/^\/api\/hoc-tap\/diem-danh\/(\d+)$/);
 if(attend&&method==='PUT'){const id=Number(attend[1]),session=sessions.find(s=>roster(s.id).some(r=>r.id===id));if(!session||new Date(session.batDau).getTime()>Date.now())throw Error('Buổi chưa bắt đầu.');const rows=roster(session.id),row=rows.find(r=>r.id===id)!;if(data.RowVersion!==row.rowVersion)throw Error('Dữ liệu đã thay đổi, tải lại danh sách.');if(!['CO_MAT','VANG'].includes(String(data.TrangThai)))throw Error('Trạng thái không hợp lệ.');Object.assign(row,{trangThai:data.TrangThai,phutThamDu:data.PhutThamDu,diemSanPham:data.DiemSanPham,nhanXet:data.NhanXet,sanPhamUrl:data.SanPhamUrl,rowVersion:String(Number(row.rowVersion)+1)});save('roster-'+key+'-'+session.id,rows);return {}}
 return undefined;
}

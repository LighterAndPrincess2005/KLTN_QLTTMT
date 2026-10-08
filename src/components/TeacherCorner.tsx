import {useEffect,useState} from 'react';
import {ArrowRight,CalendarDays,Check,RefreshCw,Users} from 'lucide-react';
import {artCenterService as api,dateTime,vnTime} from '../services/artCenterService';
import type {Member,RosterRow,TeacherSession} from '../services/artCenterService';

const errorText=(error:unknown)=>error instanceof Error?error.message:'Chưa xử lý được. Vui lòng thử lại.';
const day=(value:Date)=>value.toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'});
const today=()=>day(new Date());
const status=(value:string)=>({DA_XEP_LICH:'Đã xếp lịch',DA_TO_CHUC:'Đã hoàn thành',CHO_THAM_GIA:'Chưa điểm danh',CO_MAT:'Có mặt',VANG:'Vắng'}[value]||value);

function RosterEditor({row,session,onSaved}:{row:RosterRow;session:TeacherSession;onSaved:()=>void}){
 const [attendance,setAttendance]=useState(row.trangThai==='VANG'?'VANG':'CO_MAT');
 const [busy,setBusy]=useState(false),[error,setError]=useState(''),[notice,setNotice]=useState('');
 const minutes=Math.max(0,Math.floor((vnTime(session.ketThuc).getTime()-vnTime(session.batDau).getTime())/60000));
 const canEdit=['DA_XEP_LICH','DA_TO_CHUC'].includes(session.trangThai)&&vnTime(session.batDau).getTime()<=Date.now()&&day(vnTime(session.batDau))===today();
 const [duration,setDuration]=useState(String(row.phutThamDu??minutes)),[score,setScore]=useState(row.diemSanPham==null?'':String(row.diemSanPham));
 const [comment,setComment]=useState(row.nhanXet||''),[product,setProduct]=useState(row.sanPhamUrl||'');
 useEffect(()=>{setAttendance(row.trangThai==='VANG'?'VANG':'CO_MAT');setDuration(String(row.phutThamDu??minutes));setScore(row.diemSanPham==null?'':String(row.diemSanPham));setComment(row.nhanXet||'');setProduct(row.sanPhamUrl||'')},[row.rowVersion,minutes]);
 return <article className="teacher-student" data-student-id={row.hocVienId}>
  <div className="learning-row-head"><div><h4>{row.hoTen}</h4><p>Sinh ngày {new Date(row.ngaySinh+'T00:00:00+07:00').toLocaleDateString('vi-VN')} · {row.loaiThamGia==='HOC_BU'?'Học bù':'Học chính'}</p></div><span className={'status learning-status status-'+row.trangThai.toLowerCase()}>{status(row.trangThai)}</span></div>
  {row.luuY&&<p className="info-note">Lưu ý hỗ trợ học tập: {row.luuY}</p>}
  {row.baoNghiLuc&&<p className="teacher-absence-note">Đã báo nghỉ lúc {dateTime(row.baoNghiLuc)}{row.trangThai==='CHO_THAM_GIA'&&row.nhanXet?' · '+row.nhanXet:''}</p>}
  {canEdit?<form onSubmit={async event=>{
   event.preventDefault();setBusy(true);setError('');setNotice('');
   try{await api.recordAttendance(row.id,{TrangThai:attendance,PhutThamDu:attendance==='VANG'?0:Number(duration),DiemSanPham:score.trim()===''?null:Number(score),NhanXet:comment.trim()||null,SanPhamUrl:product.trim()||null,RowVersion:row.rowVersion});setNotice('Đã lưu điểm danh và nhận xét.');onSaved();}
   catch(error){setError(errorText(error))}finally{setBusy(false)}
  }}>
   <div className="teacher-form-grid"><label className="field"><span>Điểm danh</span><select name="attendance" value={attendance} disabled={busy} onChange={event=>setAttendance(event.target.value)}><option value="CO_MAT">Có mặt</option><option value="VANG">Vắng</option></select></label>
    <label className="field"><span>Số phút tham dự</span><input name="minutes" type="number" min={1} max={Math.min(minutes,1440)} step={1} value={attendance==='VANG'?'0':duration} required={attendance==='CO_MAT'} disabled={busy||attendance==='VANG'} onChange={event=>setDuration(event.target.value)}/></label>
    <label className="field"><span>Điểm sản phẩm (0–10)</span><input name="score" type="number" min={0} max={10} step="0.1" value={score} disabled={busy} onChange={event=>setScore(event.target.value)} placeholder="Chưa có điểm"/></label>
   </div>
   <label className="field"><span>Nhận xét buổi học</span><textarea name="comment" maxLength={2000} rows={3} value={comment} disabled={busy} onChange={event=>setComment(event.target.value)} placeholder="Điểm tiến bộ và nội dung học viên cần luyện thêm."/></label>
   <label className="field"><span>Liên kết bài thực hành (không bắt buộc)</span><input name="product" type="url" maxLength={500} value={product} disabled={busy} onChange={event=>setProduct(event.target.value)} placeholder="https://…"/></label>
   {error&&<p className="error-note" role="alert">{error}</p>}{notice&&<p className="info-note" role="status">{notice}</p>}
   <button className="btn btn-primary" disabled={busy}>{busy?'Đang lưu…':'Lưu điểm danh & nhận xét'} <Check size={16}/></button>
  </form>:<div className="teacher-readonly"><p>{row.trangThai==='CHO_THAM_GIA'?'Chưa có nhận xét đã ghi.':row.nhanXet||'Chưa có nhận xét.'}</p><p>{row.diemSanPham==null?'Chưa có điểm sản phẩm.':`Điểm sản phẩm: ${row.diemSanPham}/10`}</p>{row.sanPhamUrl&&/^https?:\/\//i.test(row.sanPhamUrl)&&<a className="text-link" href={row.sanPhamUrl} target="_blank" rel="noopener noreferrer">Xem bài thực hành <ArrowRight size={16}/></a>}</div>}
 </article>;
}

function TeachingSession({session,onChanged}:{session:TeacherSession;onChanged:()=>void}){
 const [rows,setRows]=useState<RosterRow[]>([]),[loading,setLoading]=useState(true),[error,setError]=useState(''),[notice,setNotice]=useState(''),[reload,setReload]=useState(0),[closing,setClosing]=useState(false);
 useEffect(()=>{let alive=true;setLoading(true);setError('');api.teacherRoster(session.id).then(data=>{if(alive)setRows(data)}).catch(error=>{if(alive)setError(errorText(error))}).finally(()=>{if(alive)setLoading(false)});return()=>{alive=false}},[session.id,reload]);
 const canClose=session.trangThai==='DA_XEP_LICH'&&vnTime(session.ketThuc).getTime()<=Date.now()&&rows.length>0&&rows.every(row=>row.trangThai==='CO_MAT'||row.trangThai==='VANG');
 const canEdit=vnTime(session.batDau).getTime()<=Date.now()&&day(vnTime(session.batDau))===today();
 return <section className="teacher-workspace" aria-labelledby="teacher-session-title">
  <div className="learning-heading"><div><span className="kicker">BUỔI ĐANG XEM</span><h3 id="teacher-session-title">{session.tenLop} · Buổi {session.thuTu} · {session.chuDe}</h3><p>{dateTime(session.batDau)} · {session.tenPhong}</p></div><button className="learning-refresh" disabled={loading||closing} onClick={()=>{setReload(n=>n+1);onChanged()}}><RefreshCw size={16}/> Cập nhật</button></div>
  {!canEdit&&<p className="info-note">Bạn có thể xem danh sách và kết quả đã ghi. Điểm danh/nhận xét được cập nhật sau khi buổi bắt đầu, trong ngày học; chỉnh muộn cần học vụ hỗ trợ.</p>}
  {notice&&<p className="info-note" role="status">{notice}</p>}{error&&<div className="error-note" role="alert"><p>{error}</p><button className="btn btn-soft" onClick={()=>setReload(n=>n+1)}>Tải lại danh sách</button></div>}
  {loading&&!rows.length?<p className="learning-empty" role="status">Đang tải danh sách học viên…</p>:!error&&<><div className="teacher-roster">{rows.map(row=><RosterEditor key={row.id} row={row} session={session} onSaved={()=>{setReload(n=>n+1);onChanged()}}/>)}</div>{!rows.length&&<p className="learning-empty">Buổi này chưa có học viên đã xác nhận vào lớp.</p>}
   <div className="teacher-close"><p>{session.trangThai==='DA_TO_CHUC'?'Buổi học đã được hoàn thành.':'Sau khi buổi kết thúc và tất cả học viên đã được điểm danh, bạn có thể hoàn thành buổi học.'}</p>{canClose&&<button className="btn btn-primary" disabled={closing||loading} onClick={async()=>{setClosing(true);setError('');try{await api.closeSession(session.id);setNotice('Đã hoàn thành buổi học.');onChanged();setReload(n=>n+1)}catch(error){setError(errorText(error))}finally{setClosing(false)}}}>Hoàn thành buổi học <Check size={16}/></button>}</div>
  </>}
 </section>;
}

function TeacherDashboard(){
 const [rows,setRows]=useState<TeacherSession[]>([]),[selectedId,setSelectedId]=useState<number|null>(null),[scope,setScope]=useState('all'),[query,setQuery]=useState('');
 const [loading,setLoading]=useState(true),[error,setError]=useState(''),[reload,setReload]=useState(0);
 useEffect(()=>{let alive=true;setLoading(true);setError('');api.teacherSchedule().then(data=>{if(alive){setRows(data);setSelectedId(old=>data.some(row=>row.id===old)?old:(data.find(row=>day(vnTime(row.batDau))===today())||data.find(row=>vnTime(row.batDau).getTime()>Date.now())||data.at(-1))?.id??null)}}).catch(error=>{if(alive)setError(errorText(error))}).finally(()=>{if(alive)setLoading(false)});return()=>{alive=false}},[reload]);
 const visible=rows.filter(row=>scope==='all'||scope==='today'&&day(vnTime(row.batDau))===today()||scope==='future'&&vnTime(row.batDau).getTime()>Date.now()||scope==='past'&&vnTime(row.ketThuc).getTime()<=Date.now()).filter(row=>(row.tenLop+' '+row.tenKhoa+' '+row.chuDe).toLocaleLowerCase('vi-VN').includes(query.toLocaleLowerCase('vi-VN')));
 const selected=visible.find(row=>row.id===selectedId)||visible[0];
 return <><section className="teacher-hero"><div className="shell"><span className="kicker light">LHL ART · GÓC GIÁO VIÊN</span><h1>Mỗi buổi học,<br/>một bước tiến mới.</h1><p>Lịch giảng dạy, điểm danh và nhận xét học viên trong các buổi bạn được phân công.</p><a className="btn btn-white" href="#top">Xem trang giới thiệu <ArrowRight size={17}/></a></div></section>
  <section className="section teacher-area"><div className="shell"><div className="section-head"><div><span className="kicker">LỊCH GIẢNG DẠY CỦA BẠN</span><h2>Chọn buổi để vào lớp.</h2></div><p>Chỉ các buổi được phân công xuất hiện trong danh sách này.</p></div>
   <div className="teacher-filters"><label className="field"><span>Khoảng thời gian</span><select name="teacherScope" value={scope} onChange={event=>setScope(event.target.value)}><option value="all">Tất cả buổi</option><option value="today">Hôm nay</option><option value="future">Sắp tới</option><option value="past">Đã diễn ra</option></select></label><label className="field"><span>Tìm lớp / nội dung</span><input name="teacherSearch" value={query} onChange={event=>setQuery(event.target.value)} placeholder="Nhập tên lớp, khóa hoặc bài học"/></label></div>
   {error&&<div className="error-note" role="alert"><p>{error}</p><button className="btn btn-soft" onClick={()=>setReload(n=>n+1)}>Thử lại</button></div>}
   {loading&&!rows.length?<p className="learning-empty" role="status">Đang tải lịch giảng dạy…</p>:!error&&<><div className="teacher-session-list">{visible.map(row=><button type="button" key={row.id} className={'teacher-session-button'+(selected?.id===row.id?' selected':'')} aria-pressed={selected?.id===row.id} onClick={()=>setSelectedId(row.id)}><span className="category">{row.maLop} · {row.tenKhoa}</span><b>{row.tenLop} · Buổi {row.thuTu}</b><span>{row.chuDe}</span><span><CalendarDays size={15}/>{dateTime(row.batDau)}</span><span><Users size={15}/>{row.soHocVien} học viên · {row.tenPhong}</span><small>{row.vaiTro==='CHINH'?'Giáo viên chính':'Trợ giảng'} · {status(row.trangThai)}</small></button>)}</div>{!visible.length&&<p className="learning-empty">{rows.length?'Chưa có buổi phù hợp với bộ lọc.':'Bạn chưa được phân công buổi học nào.'}</p>}
    {selected&&<TeachingSession key={selected.id} session={selected} onChanged={()=>setReload(n=>n+1)}/>}</>}
  </div></section></>;
}

export function TeacherCorner({member,onLogin}:{member:Member|null;onLogin:()=>void}){
 if(!member)return <section className="section"><div className="shell member-welcome"><h2>Góc giáo viên</h2><p>Đăng nhập bằng tài khoản giáo viên để xem buổi được phân công.</p><button className="btn btn-primary" onClick={onLogin}>Đăng nhập</button></div></section>;
 if(member.vaiTro!=='GIAO_VIEN')return <section className="section"><div className="shell learning-welcome"><h2>Góc giáo viên dành cho tài khoản giáo viên.</h2><p>Dùng Học tập để xem thông tin học viên của bạn.</p><a className="text-link" href="#member">Về góc học tập <ArrowRight size={16}/></a></div></section>;
 return <TeacherDashboard key={member.id}/>;
}

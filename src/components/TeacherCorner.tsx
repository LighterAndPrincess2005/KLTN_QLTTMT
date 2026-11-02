import {statusLabel} from '../displayLabels';
import {TeacherClassActions} from './TeacherClassActions';
import {TeacherCalendar} from './TeacherCalendar';
import './workspacePages.css';
import {safeMediaUrl} from '../mediaUrls';
import {pageHref} from '../navigation';
import {useEffect,useRef,useState} from 'react';
import {ArrowRight,BookOpen,CalendarDays,Check,RefreshCw,Users} from 'lucide-react';
import {artCenterService as api,dateTime,vnTime} from '../services/artCenterService';
import type {Member,RosterRow,TeacherSession} from '../services/artCenterService';
import {demoMode} from '../services/demo';

const errorText=(error:unknown)=>error instanceof Error?error.message:'Chưa xử lý được. Vui lòng thử lại.';
const day=(value:Date)=>value.toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'});
const today=()=>day(new Date());
const status=(value:string)=>({DA_XEP_LICH:'Đã xếp lịch',DA_TO_CHUC:'Đã hoàn thành',CHO_THAM_GIA:'Chưa điểm danh',CO_MAT:'Có mặt',VANG:'Vắng'}[value]||statusLabel(value));

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
  </form>:<div className="teacher-readonly"><p>{row.trangThai==='CHO_THAM_GIA'?'Chưa có nhận xét đã ghi.':row.nhanXet||'Chưa có nhận xét.'}</p><p>{row.diemSanPham==null?'Chưa có điểm sản phẩm.':`Điểm sản phẩm: ${row.diemSanPham}/10`}</p>{row.sanPhamUrl&&safeMediaUrl(row.sanPhamUrl)&&<a className="text-link" href={row.sanPhamUrl} target="_blank" rel="noopener noreferrer">Xem bài thực hành <ArrowRight size={16}/></a>}</div>}
 </article>;
}

function TeachingSession({session,onChanged}:{session:TeacherSession;onChanged:()=>void}){
 const [rows,setRows]=useState<RosterRow[]>([]),[loading,setLoading]=useState(true),[error,setError]=useState(''),[notice,setNotice]=useState(''),[reload,setReload]=useState(0),[closing,setClosing]=useState(false);
 useEffect(()=>{let alive=true;setLoading(true);setError('');api.teacherRoster(session.id).then(data=>{if(alive)setRows(data)}).catch(error=>{if(alive)setError(errorText(error))}).finally(()=>{if(alive)setLoading(false)});return()=>{alive=false}},[session.id,reload]);
 const canClose=session.trangThai==='DA_XEP_LICH'&&vnTime(session.ketThuc).getTime()<=Date.now()&&rows.length>0&&rows.every(row=>row.trangThai==='CO_MAT'||row.trangThai==='VANG');
 const canEdit=vnTime(session.batDau).getTime()<=Date.now()&&day(vnTime(session.batDau))===today();
 return <section className="teacher-workspace" aria-labelledby="teacher-session-title">
  <div className="learning-heading"><div><span className="kicker">BUỔI ĐANG XEM</span><h3 id="teacher-session-title">{session.tenLop} · Buổi {session.thuTu} · {session.chuDe}</h3><p>{dateTime(session.batDau)} · {session.tenPhong}</p></div><button className="learning-refresh" disabled={loading||closing} onClick={()=>{setReload(n=>n+1);onChanged()}}><RefreshCw size={16}/> Cập nhật</button></div>
  <div className="teacher-attendance-summary" aria-label="Tổng hợp điểm danh"><div><b>{rows.length}</b><span>Học viên</span></div><div><b>{rows.filter(r=>r.trangThai==='CO_MAT').length}</b><span>Có mặt</span></div><div><b>{rows.filter(r=>r.trangThai==='VANG').length}</b><span>Vắng</span></div><div><b>{rows.filter(r=>r.trangThai==='CHO_THAM_GIA').length}</b><span>Chưa điểm danh</span></div></div>
  {!canEdit&&<p className="info-note">Bạn có thể xem danh sách và kết quả đã ghi. Điểm danh/nhận xét được cập nhật sau khi buổi bắt đầu, trong ngày học; chỉnh muộn cần học vụ hỗ trợ.</p>}
  {notice&&<p className="info-note" role="status">{notice}</p>}{error&&<div className="error-note" role="alert"><p>{error}</p><button className="btn btn-soft" onClick={()=>setReload(n=>n+1)}>Tải lại danh sách</button></div>}
  {loading&&!rows.length?<p className="learning-empty" role="status">Đang tải danh sách học viên…</p>:!error&&<><div className="teacher-roster">{rows.map(row=><RosterEditor key={row.id} row={row} session={session} onSaved={()=>{setReload(n=>n+1);onChanged()}}/>)}</div>{!rows.length&&<p className="learning-empty">Buổi này chưa có học viên đã xác nhận vào lớp.</p>}
   <div className="teacher-close"><p>{session.trangThai==='DA_TO_CHUC'?'Buổi học đã được hoàn thành.':'Sau khi buổi kết thúc và tất cả học viên đã được điểm danh, bạn có thể hoàn thành buổi học.'}</p>{canClose&&<button className="btn btn-primary" disabled={closing||loading} onClick={async()=>{setClosing(true);setError('');try{await api.closeSession(session.id);setNotice('Đã hoàn thành buổi học.');onChanged();setReload(n=>n+1)}catch(error){setError(errorText(error))}finally{setClosing(false)}}}>Hoàn thành buổi học <Check size={16}/></button>}</div>
  </>}
 </section>;
}

function TeacherDashboard({member}:{member:Member}){
 const [rows,setRows]=useState<TeacherSession[]>([]),[selectedId,setSelectedId]=useState<number|null>(null),[classId,setClassId]=useState(''),[query,setQuery]=useState(''),[view,setView]=useState<'calendar'|'session'>('calendar');
 const [date,setDate]=useState(today),[loading,setLoading]=useState(true),[error,setError]=useState(''),[reload,setReload]=useState(0);
 const initialized=useRef(false);
 useEffect(()=>{if(view==='session'){const heading=document.getElementById('teacher-session-title');if(heading){heading.tabIndex=-1;heading.focus({preventScroll:true});heading.scrollIntoView({block:'start',behavior:'instant'})}}},[view,selectedId]);
 useEffect(()=>{let alive=true;setLoading(true);setError('');api.teacherSchedule().then(data=>{if(alive){const sorted=[...data].sort((a,b)=>vnTime(a.batDau).getTime()-vnTime(b.batDau).getTime());setRows(sorted);setSelectedId(old=>sorted.some(row=>row.id===old)?old:null);if(!initialized.current&&sorted.length){initialized.current=true;const next=sorted.find(row=>vnTime(row.ketThuc).getTime()>=Date.now());if(next)setDate(day(vnTime(next.batDau)));}}}).catch(error=>{if(alive)setError(errorText(error))}).finally(()=>{if(alive)setLoading(false)});return()=>{alive=false}},[reload]);
 const classes=[...new Map(rows.map(row=>[row.lopHocId,row])).values()];
 const visible=rows.filter(row=>(!classId||String(row.lopHocId)===classId)&&(row.tenLop+' '+row.tenKhoa+' '+row.chuDe).toLocaleLowerCase('vi-VN').includes(query.toLocaleLowerCase('vi-VN')));
 const selected=rows.find(row=>row.id===selectedId),todays=rows.filter(row=>day(vnTime(row.batDau))===today());
 const openSession=(row:TeacherSession)=>{setSelectedId(row.id);setView('session');};
 return <section className="study-page teacher-dashboard-page"><div className="shell"><a className="study-home-link" href={pageHref('/')}>← Về trang chủ</a><div className="study-page-heading"><div><span className="kicker">KHÔNG GIAN GIẢNG DẠY</span><h1>Chào {member.hoTen||member.tenDangNhap}.</h1><p>Lịch dạy, danh sách học viên và điểm danh trong một không gian.</p></div><button className="btn btn-soft" disabled={loading} onClick={()=>setReload(n=>n+1)}><RefreshCw size={17}/>Cập nhật lịch dạy</button></div>
 {demoMode&&<p className="info-note">Bản xem thử giáo viên · Điểm danh chỉ lưu trong phiên trình duyệt.</p>}
 <div className="study-account-stats"><div><CalendarDays size={21}/><b>{todays.length}</b><span>Buổi dạy hôm nay</span></div><div><BookOpen size={21}/><b>{classes.length}</b><span>Lớp được phân công</span></div><div><Check size={21}/><b>{rows.filter(row=>row.trangThai==='DA_TO_CHUC').length}</b><span>Buổi đã hoàn thành</span></div></div>
 <div className="study-dashboard teacher-dashboard"><aside className="study-sidebar teacher-sidebar" aria-label="Chọn lớp giảng dạy"><div className="study-profile-card"><span className="kicker">GIÁO VIÊN</span><h2>{member.hoTen||member.tenDangNhap}</h2><p>Lịch hiển thị đúng các buổi bạn được phân công.</p><a className="text-link" href={pageHref('/tai-khoan')}>Thông tin tài khoản <ArrowRight size={15}/></a></div><button className={'study-all-calendar'+(view==='calendar'&&!classId?' selected':'')} onClick={()=>{setClassId('');setQuery('');setView('calendar')}}><CalendarDays size={20}/><span>Lịch tuần tổng hợp<small>Tất cả lớp được phân công</small></span><ArrowRight size={16}/></button><div className="study-class-list"><h2>Lớp của bạn</h2>{classes.length?classes.map(row=><button type="button" className={'study-class-button'+(classId===String(row.lopHocId)?' selected':'')} key={row.lopHocId} aria-pressed={classId===String(row.lopHocId)} onClick={()=>{setClassId(String(row.lopHocId));setView('calendar');setQuery('');const upcoming=rows.find(s=>s.lopHocId===row.lopHocId&&vnTime(s.ketThuc).getTime()>=Date.now())||rows.find(s=>s.lopHocId===row.lopHocId);if(upcoming)setDate(day(vnTime(upcoming.batDau)))}}><small>Lớp được phân công</small><b>{row.tenLop}</b><span>{row.tenKhoa}</span><span>{rows.filter(s=>s.lopHocId===row.lopHocId).length} buổi được phân công</span></button>):<p className="learning-empty">Bạn chưa được phân công lớp.</p>}</div><a className="study-support teacher-policy-link" href={pageHref('/chinh-sach')}><BookOpen size={19}/><span>Quy định học tập<b>Chính sách & điều khoản <ArrowRight size={14}/></b></span></a><a className="study-support" href={pageHref('/#consultation')}><Users size={19}/><span>Cần hỗ trợ?<b>Liên hệ học vụ <ArrowRight size={14}/></b></span></a></aside>
 <div className="study-main teacher-main"><TeacherClassActions sessions={rows} selectedClass={classId} onChanged={newDate=>{if(newDate)setDate(newDate);setReload(n=>n+1)}}/>{error&&<div className="error-note" role="alert"><p>{error}</p><button className="btn btn-soft" onClick={()=>setReload(n=>n+1)}>Tải lại lịch dạy</button></div>}{loading&&!rows.length?<p className="learning-empty" role="status">Đang tải lịch giảng dạy…</p>:!error&&<>{view==='calendar'?<><label className="field teacher-calendar-search"><span>Tìm trong lịch đang xem</span><input type="search" placeholder="Tên lớp, khóa hoặc chủ đề buổi học" value={query} onChange={e=>setQuery(e.target.value)}/></label><TeacherCalendar sessions={visible} date={date} onDate={setDate} onOpen={openSession}/></>:selected?<><button type="button" className="btn btn-soft teacher-back-calendar" onClick={()=>setView('calendar')}>← Quay lại lịch tuần</button><TeachingSession key={selected.id} session={selected} onChanged={()=>setReload(n=>n+1)}/></>:<p className="learning-empty">Chọn một buổi trên lịch để xem danh sách học viên.</p>}</>}</div></div></div></section>;
}

export function TeacherCorner({member,onLogin}:{member:Member|null;onLogin:()=>void}){
 if(!member)return <section className="section"><div className="shell member-welcome"><h2>Góc giáo viên</h2><p>Đăng nhập bằng tài khoản giáo viên để xem buổi được phân công.</p>{demoMode&&<p className="info-note">Xem thử: tên đăng nhập <b>gv_demo</b>, mật khẩu <b>Demo123456789</b>. Dữ liệu chỉ được mô phỏng trong phiên trình duyệt.</p>}<button className="btn btn-primary" onClick={onLogin}>Đăng nhập</button></div></section>;
 if(member.vaiTro!=='GIAO_VIEN')return <section className="section"><div className="shell learning-welcome"><h2>Góc giáo viên dành cho tài khoản giáo viên.</h2><p>Dùng Học tập để xem thông tin học viên của bạn.</p><a className="text-link" href={pageHref('/hoc-tap')}>Về góc học tập <ArrowRight size={16}/></a></div></section>;
 return <TeacherDashboard key={member.id} member={member}/>;
}

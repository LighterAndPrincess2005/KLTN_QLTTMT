import {useEffect,useMemo,useRef,useState} from 'react';
import {ArrowLeft,ArrowRight,CalendarDays,Check,Clock3,List,MapPin,RefreshCw} from 'lucide-react';
import {artCenterService as api,vnTime} from '../services/artCenterService';
import type {Attendance,Enrollment} from '../services/artCenterService';

const zone='Asia/Ho_Chi_Minh';
const keyDate=(date:Date)=>date.toLocaleDateString('en-CA',{timeZone:zone});
const addDays=(key:string,days:number)=>{const date=new Date(key+'T12:00:00Z');date.setUTCDate(date.getUTCDate()+days);return date.toISOString().slice(0,10)};
const monday=(key:string)=>{const day=new Date(key+'T12:00:00Z').getUTCDay();return addDays(key,-((day+6)%7))};
const shortDate=(key:string)=>key.slice(8,10)+'/'+key.slice(5,7);
const time=(date:Date)=>date.toLocaleTimeString('vi-VN',{timeZone:zone,hour:'2-digit',minute:'2-digit'});
const period=(date:Date)=>{const hour=Number(new Intl.DateTimeFormat('en-GB',{timeZone:zone,hour:'2-digit',hourCycle:'h23'}).format(date));return hour<12?0:hour<18?1:2};
const palette=['#236f57','#ba532e','#586da0','#94703b','#785594','#337b85'];
const days=['Thứ Hai','Thứ Ba','Thứ Tư','Thứ Năm','Thứ Sáu','Thứ Bảy','Chủ nhật'];
type Event={attendance:Attendance;enrollment:Enrollment;start:Date;end:Date|null;day:string;period:number;conflict:boolean};

export function WeeklySchedule({studentId,studentName,enrollments,version,onOpenClass}:{studentId:string;studentName:string;enrollments:Enrollment[];version:number;onOpenClass:(id:number)=>void}){
 const [date,setDate]=useState(()=>keyDate(new Date())),[filter,setFilter]=useState('all'),[mode,setMode]=useState<'week'|'list'>(()=>window.matchMedia('(max-width:680px)').matches?'list':'week'),[kind,setKind]=useState('all');
 const [events,setEvents]=useState<Event[]>([]),[loading,setLoading]=useState(true),[failures,setFailures]=useState<string[]>([]),[attempt,setAttempt]=useState(0),[selected,setSelected]=useState<Event|null>(null);
 const dialog=useRef<HTMLDialogElement>(null),focusBeforeDialog=useRef<HTMLElement|null>(null);
 const confirmed=enrollments.filter(e=>e.trangThai==='DA_XAC_NHAN');
 const signature=confirmed.map(e=>e.id+':'+e.hoSoTheoHocId).sort().join('|');
 useEffect(()=>{
   let alive=true;setLoading(true);setEvents([]);setFailures([]);setFilter('all');setSelected(null);
   const unique=[...new Map(confirmed.map(e=>[e.hoSoTheoHocId,e])).values()];
   Promise.allSettled(unique.map(async enrollment=>({enrollment,progress:await api.progress(enrollment.hoSoTheoHocId)}))).then(results=>{
     if(!alive)return;const found:Event[]=[],failed:string[]=[],seen=new Set<string>();
     results.forEach((result,index)=>{if(result.status==='rejected'){failed.push(unique[index].tenLop);return}
       const {enrollment,progress}=result.value;
       for(const row of progress.diemDanh){if(row.trangThai==='HUY_BO'||row.trangThaiBuoi==='DA_HUY')continue;
         const start=vnTime(row.ngayHoc),end=row.ketThuc?vnTime(row.ketThuc):null;if(!Number.isFinite(start.getTime()))continue;
         const key=String(row.buoiHocId||row.id);if(seen.has(key))continue;seen.add(key);
         const actual=confirmed.find(e=>e.id===row.dangKyId)||enrollment;
         found.push({attendance:row,enrollment:actual,start,end:end&&Number.isFinite(end.getTime())?end:null,day:keyDate(start),period:period(start),conflict:false});
       }
     });
     found.sort((a,b)=>a.start.getTime()-b.start.getTime());
     for(let i=0;i<found.length;i++)for(let j=i+1;j<found.length;j++){const a=found[i],b=found[j];if(a.end&&b.end&&a.start<b.end&&b.start<a.end){a.conflict=true;b.conflict=true}}
     setEvents(found);setFailures(failed);setLoading(false);
     // Do not present an empty current week when the first class starts later.
     setDate(current=>{const from=monday(current),to=addDays(from,6);if(found.some(e=>e.day>=from&&e.day<=to))return current;return found.find(e=>e.start>=new Date())?.day||current});
   });return()=>{alive=false};
 },[studentId,signature,version,attempt]);
 useEffect(()=>{if(selected&&dialog.current&&!dialog.current.open){focusBeforeDialog.current=document.activeElement as HTMLElement;dialog.current.showModal()}else if(!selected&&dialog.current?.open)dialog.current.close()},[selected]);
 const start=monday(date),weekDays=Array.from({length:7},(_,i)=>addDays(start,i)),today=keyDate(new Date());
 const filtered=useMemo(()=>events.filter(e=>(filter==='all'||String(e.enrollment.id)===filter)&&(kind==='all'||(kind==='makeup'?e.attendance.loaiThamGia==='HOC_BU':e.attendance.loaiThamGia!=='HOC_BU'))),[events,filter,kind]);
 const week=filtered.filter(e=>e.day>=start&&e.day<=weekDays[6]);
 const color=(e:Event)=>palette[Math.max(0,confirmed.findIndex(c=>c.id===e.enrollment.id))%palette.length];
 const open=(event:Event)=>setSelected(event);
 const card=(e:Event)=><button type="button" key={e.attendance.id} className={'week-event'+(e.conflict?' conflict':'')} style={{'--class-color':color(e)} as React.CSSProperties} onClick={()=>open(e)} aria-label={`${e.attendance.tenLop||e.enrollment.tenLop}, ${time(e.start)}, ${e.day}, ${e.attendance.loaiThamGia==='HOC_BU'?'học bù':'học chính'}`}><span className="week-event-time"><Clock3 size={12}/>{time(e.start)}{e.end?'–'+time(e.end):''}</span><b>{e.attendance.tenLop||e.enrollment.tenLop}</b><span className="week-event-course">{e.enrollment.tenKhoa}</span>{e.attendance.chuDe&&<span>{e.attendance.chuDe}</span>}{e.attendance.tenPhong&&<span className="week-event-room"><MapPin size={12}/>{e.attendance.tenPhong}</span>}<span className="week-event-tags">{e.attendance.loaiThamGia==='HOC_BU'&&<em>Học bù</em>}{e.attendance.trangThai==='CO_MAT'&&<em><Check size={11}/> Có mặt</em>}{e.attendance.trangThai==='VANG'&&<em>Vắng</em>}{e.conflict&&<em className="week-conflict">Trùng giờ</em>}</span></button>;
 return <section className="weekly-schedule" aria-labelledby="weekly-title"><div className="week-heading"><div><span className="kicker">MỘT TUẦN, NHIỀU SẮC MÀU</span><h2 id="weekly-title">Lịch học của {studentName}</h2><p>Tất cả các lớp và buổi học bù của học viên, trong cùng một lịch.</p></div><button className="learning-refresh" onClick={()=>setAttempt(a=>a+1)} disabled={loading}><RefreshCw size={16}/> Cập nhật</button></div>
   <div className="week-controls"><div className="week-nav"><button aria-label="Tuần trước" onClick={()=>setDate(addDays(start,-7))}><ArrowLeft size={17}/></button><button onClick={()=>setDate(today)}>Tuần này</button><button aria-label="Tuần sau" onClick={()=>setDate(addDays(start,7))}><ArrowRight size={17}/></button></div><label className="week-date"><span>Chọn ngày</span><input aria-label="Chọn ngày xem lịch" type="date" value={date} onChange={e=>{if(e.target.value)setDate(e.target.value)}}/></label><div className="week-view-switch" role="group" aria-label="Cách xem lịch"><button aria-pressed={mode==='week'} onClick={()=>setMode('week')}><CalendarDays size={15}/> Lịch tuần</button><button aria-pressed={mode==='list'} onClick={()=>setMode('list')}><List size={15}/> Danh sách</button></div></div>
   <div className="week-filter-row"><label><span>Lớp học</span><select aria-label="Lọc lớp trong lịch" value={filter} onChange={e=>setFilter(e.target.value)}><option value="all">Tất cả lớp ({confirmed.length})</option>{confirmed.map(e=><option key={e.id} value={e.id}>{e.tenLop}</option>)}</select></label><label><span>Loại buổi</span><select aria-label="Lọc loại buổi học" value={kind} onChange={e=>setKind(e.target.value)}><option value="all">Tất cả buổi</option><option value="regular">Học chính</option><option value="makeup">Học bù</option></select></label><p role="status">{shortDate(start)}–{shortDate(weekDays[6])}/{weekDays[6].slice(0,4)} · {loading?'Đang tải…':week.length+' buổi học'}</p></div>
   <div className="week-fixed-schedules"><b>Lịch cố định của từng lớp</b>{confirmed.filter(e=>filter==='all'||String(e.id)===filter).map(e=><div key={e.id}><strong>{e.tenLop}</strong><span>{e.lichHoc||'Trung tâm chưa công bố khung giờ cố định.'}</span></div>)}<small>Lịch chính lặp theo khung giờ của lớp. Buổi học bù hoặc dời lịch có thể khác; lịch bên dưới hiển thị ngày học thực tế đã được trung tâm xếp.</small></div>
   {failures.length>0&&<div className="error-note" role="alert">Chưa tải được lịch của: {failures.join(', ')}. Lịch hiện tại có thể chưa đầy đủ. <button className="link-button" onClick={()=>setAttempt(a=>a+1)}>Thử lại</button></div>}
   {loading?<p className="learning-empty" role="status">Đang tổng hợp lịch các lớp…</p>:<>
     <div className="week-legend" aria-label="Chú giải lớp học">{confirmed.filter(e=>filter==='all'||String(e.id)===filter).map(e=><span key={e.id}><i style={{background:palette[confirmed.indexOf(e)%palette.length]}}/>{e.tenLop}</span>)}</div>
     {mode==='week'?<div className="week-table-scroll" role="region" aria-label="Lịch học theo tuần, cuộn ngang để xem đủ bảy ngày" tabIndex={0}><table className="week-table"><caption className="sr-only">Lịch tuần của {studentName}, từ {start} đến {weekDays[6]}</caption><thead><tr><th scope="col">Ca học</th>{weekDays.map((day,i)=><th scope="col" key={day} className={day===today?'today':''}><span>{days[i]}</span><b>{shortDate(day)}</b>{day===today&&<small>Hôm nay</small>}</th>)}</tr></thead><tbody>{['Sáng','Chiều','Tối'].map((p,pi)=><tr key={p}><th scope="row"><span>{['☀','◐','☾'][pi]}</span>{p}<small>{['Trước 12:00','12:00–18:00','Từ 18:00'][pi]}</small></th>{weekDays.map(day=><td key={day} className={day===today?'today':''}>{week.filter(e=>e.day===day&&e.period===pi).map(card)}</td>)}</tr>)}</tbody></table></div>:<div className="week-agenda">{weekDays.filter(day=>week.some(e=>e.day===day)).map(day=><section key={day}><h3>{days[weekDays.indexOf(day)]} <span>{shortDate(day)}</span></h3><div>{week.filter(e=>e.day===day).map(card)}</div></section>)}</div>}
     {!week.length&&<div className="week-empty"><CalendarDays size={28}/><b>{confirmed.length?'Chưa có buổi học trong tuần này.':'Chưa có lớp đã xác nhận để tổng hợp lịch.'}</b><p>{events.length?'Bạn có thể chuyển tuần hoặc chọn ngày khác.':'Lịch sẽ xuất hiện khi các buổi học được xếp lịch.'}</p>{filtered.find(e=>e.start>=new Date())&&<button className="btn btn-soft" onClick={()=>setDate(filtered.find(e=>e.start>=new Date())!.day)}>Đến buổi học gần nhất <ArrowRight size={15}/></button>}</div>}
     <p className="week-help">Bấm vào một buổi để xem chi tiết. Màu nhận diện từng lớp; nhãn “Trùng giờ” xuất hiện khi các buổi thực tế chồng lấn.</p>
   </>}
   <dialog className="week-event-dialog" ref={dialog} aria-labelledby="week-event-title" onCancel={()=>setSelected(null)} onClose={()=>{setSelected(null);focusBeforeDialog.current?.focus()}} onClick={e=>{if(e.target===e.currentTarget)setSelected(null)}}>{selected&&<div><span className="kicker">CHI TIẾT BUỔI HỌC</span><h2 id="week-event-title">{selected.attendance.tenLop||selected.enrollment.tenLop}</h2><p>{selected.enrollment.tenKhoa}</p><dl><dt>Ngày học</dt><dd>{selected.start.toLocaleDateString('vi-VN',{timeZone:zone})}</dd><dt>Thời gian</dt><dd>{time(selected.start)}{selected.end?'–'+time(selected.end):' · Chưa cập nhật giờ kết thúc'}</dd><dt>Chủ đề</dt><dd>{selected.attendance.chuDe||'Chưa cập nhật'}</dd><dt>Phòng học</dt><dd>{selected.attendance.tenPhong||'Chưa cập nhật'}</dd><dt>Giáo viên</dt><dd>{selected.attendance.giaoVien||'Chưa cập nhật'}</dd><dt>Loại buổi</dt><dd>{selected.attendance.loaiThamGia==='HOC_BU'?'Học bù':'Học chính'}</dd></dl>{selected.conflict&&<p className="error-note">Buổi này trùng thời gian với buổi khác trong lịch học viên. Vui lòng liên hệ trung tâm để kiểm tra.</p>}<div className="week-dialog-actions"><button className="btn btn-soft" onClick={()=>setSelected(null)}>Đóng</button><button className="btn btn-primary" onClick={()=>{onOpenClass(selected.enrollment.id);setSelected(null)}}>Xem lớp & học tập <ArrowRight size={16}/></button></div></div>}</dialog>
 </section>;
}

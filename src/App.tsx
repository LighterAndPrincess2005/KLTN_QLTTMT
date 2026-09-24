import { useMemo, useState } from 'react';
import {
  ArrowRight, CalendarDays, Check, ChevronDown, Clock3, CreditCard,
  Heart, MapPin, Menu, Palette, Play, Search, ShieldCheck, Sparkles,
  Star, Users, X, Zap, BookOpen, CircleUserRound, MessageCircle,
} from 'lucide-react';
import { classes, courses } from './data/mockData';
import { artCenterService } from './services/artCenterService';
import type { ArtClass, Course, RegistrationDraft } from './types';
import FloatingBotanicals from './animations/FloatingBotanicals';

const money = (value:number) => new Intl.NumberFormat('vi-VN').format(value) + 'đ';

function Logo(){return <a className="logo" href="#top" aria-label="Artora - Trang chủ"><span className="logo-mark"><Palette size={22}/></span><span><b>ARTORA</b><small>creative studio</small></span></a>}

function Header({onRegister}:{onRegister:()=>void}){
  const [open,setOpen]=useState(false);
  return <><header className="site-header"><div className="shell nav-wrap"><Logo/>
    <button className="menu-button" aria-label="Mở menu" onClick={()=>setOpen(!open)}>{open?<X/>:<Menu/>}</button>
    <nav className={open?'nav open':'nav'}>
      <a href="#programs">Khóa học</a><a href="#classes">Lịch khai giảng</a><a href="#method">Phương pháp</a><a href="#gallery">Góc sáng tạo</a>
      <button className="btn btn-primary nav-cta" onClick={onRegister}>Đăng ký học thử <ArrowRight size={17}/></button>
    </nav>
  </div></header><FloatingBotanicals/></>
}

function Hero({onRegister}:{onRegister:()=>void}){
  return <section className="hero" id="top"><div className="hero-orb orb-one"/><div className="hero-orb orb-two"/>
    <div className="shell hero-grid"><div className="hero-copy">
      <span className="eyebrow"><Sparkles size={16}/> Mỹ thuật sáng tạo cho trẻ 3–17 tuổi</span>
      <h1>Mỗi nét vẽ,<br/><em>một thế giới</em><br/>được mở ra.</h1>
      <p>Không chỉ học vẽ — con học cách quan sát, kể chuyện và tự tin thể hiện điều khác biệt của riêng mình.</p>
      <div className="hero-actions"><button className="btn btn-primary btn-large" onClick={onRegister}>Tìm lớp phù hợp <ArrowRight size={19}/></button><a className="text-link" href="#method"><span className="play"><Play size={15} fill="currentColor"/></span> Khám phá Artora</a></div>
      <div className="trust-row"><div className="avatars"><span>AN</span><span>MH</span><span>KL</span><span>+2k</span></div><div><div className="stars">★★★★★</div><small>Được tin chọn bởi hơn 2.000 gia đình</small></div></div>
    </div>
    <div className="hero-art"><div className="paper-card"><img src="https://images.unsplash.com/photo-1596548438137-d51ea5c83ca5?auto=format&fit=crop&w=1000&q=90" alt="Bé sáng tạo với màu vẽ"/><div className="tape tape-a"/><div className="tape tape-b"/></div>
      <div className="floating-note note-a"><span className="note-icon coral"><Heart size={20}/></span><div><b>Học thử miễn phí</b><small>Trải nghiệm trước khi chọn lớp</small></div></div>
      <div className="floating-note note-b"><span className="note-icon yellow"><Zap size={20}/></span><div><b>Lớp học nhỏ</b><small>Tối đa 12 học viên</small></div></div>
      <span className="doodle d1">✦</span><span className="doodle d2">⌁</span><span className="doodle d3">●</span>
    </div></div>
  </section>
}

function CourseCard({course,onPick}:{course:Course,onPick:(c:Course)=>void}){
  return <article className="course-card"><div className="course-image"><img src={course.image} alt={course.title}/><span className="age-pill">{course.age}</span><button className="heart-button" aria-label="Yêu thích"><Heart size={18}/></button></div>
    <div className="course-content"><span className="category" style={{color:course.color}}>{course.category}</span><h3>{course.title}</h3><p>{course.skills.join(' · ')}</p>
      <div className="course-meta"><span><Clock3 size={15}/>{course.sessions} buổi</span><strong>{money(course.price)}</strong></div>
      <button className="course-link" onClick={()=>onPick(course)}>Xem lớp khai giảng <ArrowRight size={16}/></button></div></article>
}

function Programs({onPick}:{onPick:(c:Course)=>void}){
  const [filter,setFilter]=useState('Tất cả');
  const shown=filter==='Tất cả'?courses:courses.filter(c=>c.category===filter);
  return <section className="section programs" id="programs"><div className="shell"><div className="section-head"><div><span className="kicker">CHƯƠNG TRÌNH HỌC</span><h2>Một hành trình vừa vặn<br/>cho từng độ tuổi.</h2></div><p>Mỗi chương trình được thiết kế theo giai đoạn phát triển, để con tiến bộ tự nhiên mà vẫn giữ trọn niềm vui sáng tạo.</p></div>
    <div className="filter-row">{['Tất cả','Nền tảng','Hội họa','Thủ công'].map(x=><button key={x} onClick={()=>setFilter(x)} className={filter===x?'active':''}>{x}</button>)}</div>
    <div className="course-grid">{shown.map(c=><CourseCard key={c.id} course={c} onPick={onPick}/>)}</div>
  </div></section>
}

function Finder({onSelect}:{onSelect:(c:ArtClass)=>void}){
  const [age,setAge]=useState('Tất cả'); const [mode,setMode]=useState('Tất cả'); const [query,setQuery]=useState('');
  const shown=useMemo(()=>classes.filter(cl=>{const c=courses.find(x=>x.id===cl.courseId)!;return(age==='Tất cả'||c.age.includes(age))&&(mode==='Tất cả'||cl.mode===mode)&&(`${c.title} ${cl.teacher}`.toLowerCase().includes(query.toLowerCase()))}),[age,mode,query]);
  return <section className="section class-finder" id="classes"><div className="shell"><div className="finder-panel"><div className="finder-title"><span className="kicker light">LỊCH KHAI GIẢNG</span><h2>Tìm một lớp thật hợp với con.</h2><p>Lọc nhanh theo độ tuổi và hình thức học. Chỗ trống được cập nhật trên dữ liệu mẫu.</p></div>
    <div className="finder-controls"><label><Search size={18}/><input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Tên khóa học hoặc giáo viên"/></label><label><Users size={18}/><select value={age} onChange={e=>setAge(e.target.value)}><option>Tất cả</option><option>3–5</option><option>6–8</option><option>9–12</option><option>13–17</option></select><ChevronDown size={16}/></label><label><MapPin size={18}/><select value={mode} onChange={e=>setMode(e.target.value)}><option>Tất cả</option><option>Trực tiếp</option><option>Trực tuyến</option></select><ChevronDown size={16}/></label></div>
    <div className="class-list">{shown.map(cl=>{const c=courses.find(x=>x.id===cl.courseId)!;const left=cl.capacity-cl.enrolled;return <div className="class-row" key={cl.id}><div className="class-color" style={{background:c.color}}/><div className="class-main"><small>{cl.id} · {c.age}</small><h3>{c.title}</h3><span><CalendarDays size={15}/>{cl.schedule}</span></div><div className="class-detail"><small>Khai giảng</small><b>{cl.startDate}</b><span>{cl.teacher}</span></div><div className="class-detail"><small>Địa điểm</small><b>{cl.room}</b><span>{cl.mode}</span></div><div className="seat"><strong>{left}</strong><small>chỗ trống</small></div><button onClick={()=>onSelect(cl)}>Chọn lớp <ArrowRight size={16}/></button></div>})}</div>
    {shown.length===0&&<div className="empty">Không có lớp phù hợp với bộ lọc hiện tại.</div>}
  </div></div></section>
}

function Method(){return <section className="section method" id="method"><div className="shell method-grid"><div className="method-image"><img src="https://images.unsplash.com/photo-1549490349-8643362247b5?auto=format&fit=crop&w=1000&q=90" alt="Tác phẩm hội họa nhiều màu sắc"/><div className="mini-card"><b>01 tác phẩm</b><span>hoàn chỉnh mỗi chủ đề</span></div></div><div className="method-copy"><span className="kicker">CÁCH ARTORA ĐỒNG HÀNH</span><h2>Học kỹ thuật.<br/>Giữ nguyên trí tưởng tượng.</h2><p>Giáo viên không vẽ thay hay đưa một “đáp án đẹp”. Con được hướng dẫn từng kỹ thuật nền tảng, rồi tự chọn câu chuyện, màu sắc và cách thể hiện của mình.</p><div className="method-list"><div><span>01</span><div><b>Quan sát & khám phá</b><p>Khởi động bằng câu chuyện, vật thật và trải nghiệm giác quan.</p></div></div><div><span>02</span><div><b>Thử nghiệm chất liệu</b><p>Học cách dùng màu, cọ, đất sét và kỹ thuật phù hợp lứa tuổi.</p></div></div><div><span>03</span><div><b>Sáng tạo & phản hồi</b><p>Hoàn thiện tác phẩm và chia sẻ ý tưởng bằng ngôn ngữ của con.</p></div></div></div></div></div></section>}

function Gallery(){const imgs=['photo-1577083288073-40892c0860a4','photo-1547891654-e66ed7ebb968','photo-1550859492-d5da9d8e45f3','photo-1541961017774-22349e4a1262'];return <section className="section gallery" id="gallery"><div className="shell"><div className="gallery-head"><div><span className="kicker">GÓC SÁNG TẠO</span><h2>Những thế giới nhỏ<br/>do chính các em tạo nên.</h2></div><a href="#programs">Xem chương trình <ArrowRight size={17}/></a></div><div className="gallery-grid">{imgs.map((id,i)=><figure key={id} className={`g${i+1}`}><img src={`https://images.unsplash.com/${id}?auto=format&fit=crop&w=900&q=85`} alt="Tác phẩm sáng tạo"/><figcaption>{['Màu của khu vườn','Thành phố tương lai','Chuyện của biển','Những người bạn nhỏ'][i]}<small>{['Foundation · 7 tuổi','Basic · 11 tuổi','Acrylic · 14 tuổi','Junior · 5 tuổi'][i]}</small></figcaption></figure>)}</div></div></section>}

function Testimonials(){return <section className="section testimonials"><div className="shell"><span className="kicker">PHỤ HUYNH CHIA SẺ</span><div className="quote">“Sau ba tháng, điều mình vui nhất không phải là con vẽ đẹp hơn — mà là con biết kể về bức tranh của mình bằng một sự tự tin rất khác.”</div><div className="quote-person"><span>TM</span><div><b>Chị Thu Minh</b><small>Phụ huynh bé Bảo An · Foundation</small></div><div className="stars">★★★★★</div></div></div></section>}

const emptyDraft:RegistrationDraft={courseId:'',classId:'',studentName:'',birthDate:'',guardianName:'',phone:'',email:'',note:''};
function RegistrationModal({initialCourse,initialClass,onClose}:{initialCourse?:Course|null;initialClass?:ArtClass|null;onClose:()=>void}){
  const [step,setStep]=useState(1); const [draft,setDraft]=useState<RegistrationDraft>({...emptyDraft,courseId:initialCourse?.id||initialClass?.courseId||'',classId:initialClass?.id||''}); const [result,setResult]=useState<{id:string}|null>(null); const [loading,setLoading]=useState(false);
  const selectedCourse=courses.find(c=>c.id===draft.courseId); const available=classes.filter(c=>c.courseId===draft.courseId);
  const update=(key:keyof RegistrationDraft,value:string)=>setDraft(x=>({...x,[key]:value}));
  const submit=async()=>{setLoading(true);const res=await artCenterService.submitRegistration(draft);setResult({id:res.id});setLoading(false);setStep(4)};
  return <div className="modal-backdrop" onMouseDown={e=>e.target===e.currentTarget&&onClose()}><div className="modal"><button className="modal-close" onClick={onClose}><X/></button><div className="modal-head"><Logo/><div className="steps">{[1,2,3].map(n=><span key={n} className={step>=n?'done':''}>{step>n?<Check size={14}/>:n}</span>)}</div></div>
    {step===1&&<div className="modal-body"><span className="kicker">BƯỚC 1/3</span><h2>Chọn hành trình của con</h2><p>Chọn chương trình và lớp đang phù hợp nhất. Bạn có thể thay đổi trước khi xác nhận.</p><label className="field"><span>Chương trình</span><select value={draft.courseId} onChange={e=>{update('courseId',e.target.value);update('classId','')}}><option value="">Chọn khóa học</option>{courses.map(c=><option value={c.id} key={c.id}>{c.title} · {c.age}</option>)}</select></label><label className="field"><span>Lớp khai giảng</span><select value={draft.classId} onChange={e=>update('classId',e.target.value)} disabled={!draft.courseId}><option value="">Chọn lịch học</option>{available.map(c=><option value={c.id} key={c.id}>{c.schedule} · {c.teacher}</option>)}</select></label>{selectedCourse&&<div className="price-summary"><span>Học phí dự kiến</span><b>{money(selectedCourse.price+selectedCourse.materialFee)}</b><small>Đã gồm {money(selectedCourse.materialFee)} phí họa cụ</small></div>}<button className="btn btn-primary full" disabled={!draft.classId} onClick={()=>setStep(2)}>Tiếp tục <ArrowRight size={18}/></button></div>}
    {step===2&&<div className="modal-body"><span className="kicker">BƯỚC 2/3</span><h2>Thông tin học viên</h2><p>Thông tin giúp Artora kiểm tra độ tuổi và tư vấn lộ trình phù hợp.</p><div className="form-grid"><label className="field wide"><span>Họ và tên học viên</span><input value={draft.studentName} onChange={e=>update('studentName',e.target.value)} placeholder="Nguyễn Bảo An"/></label><label className="field"><span>Ngày sinh</span><input type="date" value={draft.birthDate} onChange={e=>update('birthDate',e.target.value)}/></label><label className="field"><span>Người giám hộ</span><input value={draft.guardianName} onChange={e=>update('guardianName',e.target.value)} placeholder="Nguyễn Thu Minh"/></label><label className="field"><span>Số điện thoại</span><input value={draft.phone} onChange={e=>update('phone',e.target.value)} placeholder="09xx xxx xxx"/></label><label className="field"><span>Email</span><input type="email" value={draft.email} onChange={e=>update('email',e.target.value)} placeholder="phuhuynh@email.com"/></label></div><div className="modal-actions"><button className="btn btn-soft" onClick={()=>setStep(1)}>Quay lại</button><button className="btn btn-primary" disabled={!draft.studentName||!draft.birthDate||!draft.guardianName||!draft.phone} onClick={()=>setStep(3)}>Kiểm tra đăng ký <ArrowRight size={18}/></button></div></div>}
    {step===3&&<div className="modal-body"><span className="kicker">BƯỚC 3/3</span><h2>Xác nhận giữ chỗ</h2><p>Đăng ký được giữ trong 48 giờ. Đây là bản giao diện dùng dữ liệu giả, chưa phát sinh thanh toán thật.</p><div className="confirm-card"><div><BookOpen/><span>Chương trình</span><b>{selectedCourse?.title}</b></div><div><CalendarDays/><span>Lịch học</span><b>{classes.find(c=>c.id===draft.classId)?.schedule}</b></div><div><CircleUserRound/><span>Học viên</span><b>{draft.studentName}</b></div><div><CreditCard/><span>Tạm tính</span><b>{money((selectedCourse?.price||0)+(selectedCourse?.materialFee||0))}</b></div></div><label className="check-line"><input type="checkbox" defaultChecked/><span>Tôi đã đọc chính sách giữ chỗ, đổi/hủy lớp và đồng ý để Artora liên hệ tư vấn.</span></label><div className="modal-actions"><button className="btn btn-soft" onClick={()=>setStep(2)}>Quay lại</button><button className="btn btn-primary" onClick={submit} disabled={loading}>{loading?'Đang tạo đăng ký…':'Xác nhận đăng ký'} <ShieldCheck size={18}/></button></div></div>}
    {step===4&&<div className="modal-body success"><span className="success-icon"><Check size={34}/></span><span className="kicker">ĐĂNG KÝ THÀNH CÔNG</span><h2>Artora đã giữ chỗ cho bạn!</h2><p>Mã đăng ký <b>{result?.id}</b>. Tư vấn viên sẽ liên hệ trong một ngày làm việc để xác nhận học thử và hướng dẫn bước tiếp theo.</p><div className="hold-note"><Clock3/><div><b>Thời gian giữ chỗ: 48 giờ</b><span>Trạng thái: CHỜ XÁC NHẬN</span></div></div><button className="btn btn-primary full" onClick={onClose}>Hoàn tất</button></div>}
  </div></div>
}

function Footer(){return <footer><div className="shell footer-grid"><div><Logo/><p>Không gian mỹ thuật nơi mỗi đứa trẻ được học đúng kỹ thuật và lớn lên cùng trí tưởng tượng của riêng mình.</p></div><div><b>Khám phá</b><a href="#programs">Chương trình</a><a href="#classes">Lịch khai giảng</a><a href="#method">Phương pháp</a></div><div><b>Hỗ trợ</b><a href="#top">Chính sách học bù</a><a href="#top">Đổi & bảo lưu</a><a href="#top">Câu hỏi thường gặp</a></div><div><b>Liên hệ</b><span>36 Tân Thắng, Tây Thạnh, TP.HCM</span><span>0901 676 782</span><span>hello@artora.edu.vn</span></div></div><div className="shell footer-bottom"><span>© 2026 Artora Creative Studio</span><span>Made with color & curiosity.</span></div></footer>}

export default function App(){
  const [modal,setModal]=useState(false); const [course,setCourse]=useState<Course|null>(null); const [artClass,setArtClass]=useState<ArtClass|null>(null);
  const openCourse=(c:Course)=>{setCourse(c);setArtClass(null);setModal(true)}; const openClass=(c:ArtClass)=>{setArtClass(c);setCourse(null);setModal(true)}; const openBlank=()=>{setCourse(null);setArtClass(null);setModal(true)};
  return <><Header onRegister={openBlank}/><main><Hero onRegister={openBlank}/><Programs onPick={openCourse}/><Finder onSelect={openClass}/><Method/><Gallery/><Testimonials/><section className="cta"><div className="shell cta-inner"><div><span className="kicker light">BẮT ĐẦU NHẸ NHÀNG</span><h2>Một buổi học thử.<br/>Một cánh cửa mới.</h2><p>Để con trải nghiệm không gian, gặp giáo viên và tự cảm nhận niềm vui sáng tạo trước khi chọn khóa.</p></div><button className="btn btn-white btn-large" onClick={openBlank}>Đặt lịch học thử <ArrowRight size={19}/></button></div></section></main><Footer/>{modal&&<RegistrationModal initialCourse={course} initialClass={artClass} onClose={()=>setModal(false)}/>}<button className="chat-fab" aria-label="Tư vấn"><MessageCircle/></button></>
}

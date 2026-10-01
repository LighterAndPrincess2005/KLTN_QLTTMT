import { useEffect, useRef, useState } from 'react';
import { ArrowRight, ChevronDown, X } from 'lucide-react';
import type { ArtClass, Course } from '../types';
import { request } from '../services/artCenterService';

type Lesson = {id:number;thuTu:number;chuDe:string;noiDung:string;soTiet:number;yeuCauSanPham:string|null};
const money = (value:number) => value.toLocaleString('vi-VN')+' đ';

export function CourseDetails({course,classes,onClose}:{course:Course;classes:ArtClass[];onClose:()=>void}) {
  const [lessons,setLessons]=useState<Lesson[]>([]);
  const [loading,setLoading]=useState(true);
  const [error,setError]=useState('');
  const [attempt,setAttempt]=useState(0);
  const heading=useRef<HTMLHeadingElement>(null);
  useEffect(()=>{heading.current?.focus();heading.current?.scrollIntoView({block:'start',behavior:'auto'});},[course.id]);
  useEffect(()=>{
    let active=true;setLoading(true);setError('');setLessons([]);
    request<Lesson[]>(`/api/khoa-hoc/${encodeURIComponent(course.id)}/noi-dung`)
      .then(data=>{if(active)setLessons([...data].sort((a,b)=>a.thuTu-b.thuTu));})
      .catch(e=>{if(active)setError(e instanceof Error?e.message:'Chưa tải được nội dung khóa.');})
      .finally(()=>{if(active)setLoading(false);});
    return()=>{active=false;};
  },[course.id,attempt]);
  const upcoming=classes.filter(c=>c.courseId===course.id&&c.startDateISO&&c.startDateISO>=new Date().toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'}));
  return <article className="course-exploration" aria-labelledby="course-detail-title">
    <div className="exploration-heading"><div><span className="kicker">KHÁM PHÁ KHÓA HỌC</span><h3 id="course-detail-title" ref={heading} tabIndex={-1}>{course.title}</h3></div><button className="exploration-close" onClick={onClose} aria-label="Thu gọn chi tiết khóa"><X size={18}/> Thu gọn</button></div>
    <div className="exploration-summary"><img src={course.image} alt={`Chất liệu và cảm hứng cho ${course.title}`}/><div><h4>Con sẽ học gì, dùng gì?</h4><p>{course.skills.join(' ')}</p><h4>Khóa này phù hợp với ai?</h4><p>{course.ageMin?`Học viên từ ${course.ageMin} tuổi.`:'Học viên yêu thích mỹ thuật.'} {course.prerequisites.length?course.prerequisites.join(' '):'Trung tâm tư vấn hướng học và đánh giá đầu vào khi xác minh hồ sơ.'}</p></div></div>
    <div className="exploration-facts"><div><small>LỘ TRÌNH</small><b>{course.sessions} buổi</b></div><div><small>MỖI BUỔI</small><b>{course.minutesPerSession} phút</b></div><div><small>TỔNG THỜI LƯỢNG</small><b>{course.sessions*course.minutesPerSession/60} giờ</b></div><div><small>HỌC PHÍ KHÓA</small><b>{money(course.tuitionFee)}</b></div></div>
    <div className="exploration-columns"><section><h4>Trong từng buổi học</h4><p className="exploration-hint">Bấm vào buổi để xem nội dung và bài thực hành.</p>{loading?<p role="status">Đang tải lộ trình…</p>:error?<div role="alert"><p>{error}</p><button className="btn btn-soft" onClick={()=>setAttempt(x=>x+1)}>Thử lại</button></div>:lessons.length?lessons.map(l=><details className="lesson-outline" key={l.id}><summary><span><small>Buổi {l.thuTu} · {l.soTiet} tiết</small><b>{l.chuDe}</b></span><ChevronDown size={18}/></summary><p>{l.noiDung}</p>{l.yeuCauSanPham&&<p><strong>Bài thực hành: </strong>{l.yeuCauSanPham}</p>}</details>):<p>Trung tâm đang cập nhật nội dung từng buổi.</p>}</section>
    <section><h4>Lịch lớp sắp khai giảng</h4><p className="exploration-hint">Học phí và lịch áp dụng theo lớp bạn chọn.</p>{upcoming.length?upcoming.map(c=><article className="exploration-class" key={c.id}><b>{c.title||c.code}</b><p>{c.schedule}</p><p>Khai giảng: <strong>{c.startDate}</strong></p><p>{money(c.tuitionFee??course.tuitionFee)} · {c.status}</p><small>Đóng đăng ký: {c.registrationDeadline}</small></article>):<p>Chưa có lịch khai giảng mới cho khóa này.</p>}<a className="btn btn-primary" href="#classes">Xem lớp và đăng ký <ArrowRight size={16}/></a></section></div>
  </article>;
}

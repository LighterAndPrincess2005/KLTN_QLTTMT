import {useEffect,useRef,useState} from 'react';
import {ArrowRight,Search,X} from 'lucide-react';
import type {ArtClass,Course} from '../types';
import {courseClassesHref,courseHref,pageHref} from '../navigation';
import {isPublicClass} from '../publicClassVisibility';
import {useArticles} from '../services/websiteService';
import './siteSearch.css';

type Entry={title:string;description:string;href:string;keywords:string};
const normalize=(text:string)=>text.normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/đ/g,'d').replace(/Đ/g,'D').toLowerCase();
const sections:Entry[]=[
 {title:'Góc tư vấn',description:'Hỏi về khóa học, học phí và lịch học',href:'/#consultation',keywords:'tu van chuc tu ho tro hoi dap lien he'},
 {title:'Khóa học',description:'Chương trình, chất liệu và học phí',href:'/khoa-hoc',keywords:'khoa chuong trinh hoc phi'},
 {title:'Lớp học',description:'Lịch khai giảng, chỗ trống và đăng ký',href:'/lop-hoc#classes',keywords:'lop lich sap het cho dang ky'},
 {title:'Góc học tập',description:'Lịch học, điểm danh, kết quả và phản hồi',href:'/hoc-tap',keywords:'hoc tap thoi khoa bieu lich hoc feedback danh gia diem danh thanh toan'},
 {title:'Góc giáo viên',description:'Lịch giảng dạy, điểm danh và nhận xét học viên',href:'/giao-vien',keywords:'giao vien lich day giang day diem danh'},
 {title:'Tài khoản của bạn',description:'Thông tin cá nhân và bảo mật',href:'/tai-khoan',keywords:'tai khoan bao mat doi mat khau thong tin ca nhan'},
 {title:'Giới thiệu Bụi',description:'Giáo viên và câu chuyện trung tâm',href:'/gioi-thieu',keywords:'giao vien gioi thieu trung tam'},
 {title:'Phương pháp học',description:'Cách Bụi hướng dẫn và thực hành',href:'/phuong-phap',keywords:'phuong phap ky thuat chat lieu'},
 {title:'Góc sáng tạo',description:'Tác phẩm và cảm hứng nghệ thuật',href:'/goc-sang-tao',keywords:'tranh tac pham thanh pham sang tao'},
 {title:'Câu hỏi thường gặp',description:'Giải đáp trước khi đăng ký',href:'/#faq',keywords:'cau hoi faq quy dinh'},
 {title:'Chính sách & điều khoản',description:'Đăng ký, học phí, nghỉ học, học bù và hoàn tiền',href:'/chinh-sach',keywords:'chinh sach dieu khoan quy dinh dang ky hoc bu nghi hoc hoan tien'},
 {title:'Liên hệ trung tâm',description:'Địa chỉ, số điện thoại và email',href:'/#contact',keywords:'sdt dien thoai gmail email dia chi lien he'},
];
export function SiteSearch({courses,classes}:{courses:Course[];classes:ArtClass[]}){
 const {items:articles}=useArticles();const [query,setQuery]=useState(''),[open,setOpen]=useState(false),[active,setActive]=useState(0);const root=useRef<HTMLDivElement>(null);
 const tokens=normalize(query).trim().split(/\s+/).filter(Boolean);
 const entries:Entry[]=[...sections,...courses.map(c=>({title:c.title,description:'Khám phá chương trình khóa học',href:courseHref(c.id),keywords:c.skills.join(' ')})),...classes.filter(c=>isPublicClass(c)).map(c=>({title:c.title||'Lớp học',description:c.schedule,href:courseClassesHref(c.courseId),keywords:c.teacher||''})),...articles.map(a=>({title:a.title,description:'Bài viết nghệ thuật',href:pageHref('/bai-viet/'+a.slug),keywords:a.intro||''}))];
 const results=(tokens.length?entries.filter(e=>tokens.every(t=>normalize(e.title+' '+e.description+' '+e.keywords).includes(t))):sections.slice(0,5)).slice(0,7);
 useEffect(()=>{const close=(e:PointerEvent)=>{if(!root.current?.contains(e.target as Node))setOpen(false)};document.addEventListener('pointerdown',close);return()=>document.removeEventListener('pointerdown',close)},[]);
 const choose=(entry:Entry)=>{setOpen(false);window.location.assign(pageHref(entry.href));};
 return <div className="site-search-band"><div className="shell site-search-wrap" ref={root}><form role="search" onSubmit={e=>{e.preventDefault();if(results.length)choose(results[Math.min(active,results.length-1)])}}><Search size={19} aria-hidden="true"/><input role="combobox" aria-label="Tìm kiếm trên website" aria-expanded={open} aria-controls="site-search-results" aria-autocomplete="list" aria-activedescendant={open&&results.length?'site-result-'+active:undefined} value={query} onFocus={()=>setOpen(true)} onChange={e=>{setQuery(e.target.value);setActive(0);setOpen(true)}} onKeyDown={e=>{if(e.key==='Escape')setOpen(false);if(e.key==='ArrowDown'){e.preventDefault();setOpen(true);setActive(i=>Math.min(i+1,Math.max(0,results.length-1)))}if(e.key==='ArrowUp'){e.preventDefault();setActive(i=>Math.max(0,i-1))}}} placeholder="Bạn muốn tìm gì? Thử “tư vấn”, “Junior”, “lịch học”…" autoComplete="off"/>{query&&<button type="button" className="search-clear" aria-label="Xóa tìm kiếm" onClick={()=>{setQuery('');setActive(0);root.current?.querySelector('input')?.focus()}}><X size={16}/></button>}<button type="submit" className="search-submit">Tìm <ArrowRight size={16}/></button></form>{open&&<div className="site-search-results" id="site-search-results" role="listbox" aria-label="Gợi ý tìm kiếm">{results.length?results.map((e,i)=><div key={e.href+e.title} role="option" id={'site-result-'+i} aria-selected={i===active} onMouseDown={event=>event.preventDefault()} onClick={()=>choose(e)} onMouseEnter={()=>setActive(i)}><Search size={16}/><span><b>{e.title}</b><small>{e.description}</small></span><ArrowRight size={16}/></div>):<p role="status">Chưa tìm thấy. Thử tên khóa học, “tư vấn” hoặc “lịch học”.</p>}<p className="search-keyboard-hint">↑ ↓ chọn gợi ý · Enter để đến mục · Esc để đóng</p></div>}</div></div>;
}

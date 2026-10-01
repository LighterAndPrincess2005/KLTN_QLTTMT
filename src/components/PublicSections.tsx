import { useState } from 'react';
import { ArrowRight, BookOpen, ChevronDown, Palette } from 'lucide-react';
import type { ArtClass, Course } from '../types';
import { CourseDetails } from './CourseDetails';

export function IntroPrograms({courses,classes}:{courses:Course[];classes:ArtClass[]}) {
  const [category, setCategory] = useState('Tất cả');
  const [selected, setSelected] = useState<string | null>(null);
  const program = courses.find(c => c.id === selected);
  return <section className="section programs" id="programs"><div className="shell">
    <div className="program-spotlight"><div className="spotlight-copy"><span className="spotlight-label"><Palette size={16}/> KHÁM PHÁ CHƯƠNG TRÌNH</span><h2>Từ nét vẽ đầu tiên.<br/><span>Đến thế giới của riêng con.</span></h2><p>Mỗi chất liệu mở ra một cách sáng tạo. Tìm chương trình vừa với độ tuổi và hành trình của con.</p><a href="#program-selection" className="spotlight-button">Tìm khóa học cho con <ArrowRight size={18}/></a></div><div className="spotlight-art" aria-hidden="true"><span className="art-orbit orbit-one"/><span className="art-orbit orbit-two"/><Palette size={100} strokeWidth={1.2}/><span className="art-note">Một nét vẽ nhỏ.<br/>Vô vàn ý tưởng lớn.</span></div><div className="spotlight-path"><span><b>01</b> Khám phá chất liệu</span><span><b>02</b> Chọn hướng học</span><span><b>03</b> Tìm lớp phù hợp</span></div></div>
    <div className="program-selection-head" id="program-selection"><div><span className="kicker">CHƯƠNG TRÌNH HỌC</span><h3>Con sẽ bắt đầu từ đâu?</h3></div><p>Chọn một chương trình để khám phá lộ trình.</p></div>
    <div className="filter-row" aria-label="Nhóm chương trình">{['Tất cả', ...new Set(courses.map(c=>c.category))].map(x => <button type="button" aria-pressed={category === x} className={category === x ? 'active' : ''} key={x} onClick={() => setCategory(x)}>{x}</button>)}</div>
    <div className="course-grid">{courses.filter(c => category === 'Tất cả' || c.category === category).map(c => <article className="course-card intro-course" key={c.id}>
      <div className="course-image"><img src={c.image} alt={`Khám phá chất liệu trong chương trình ${c.title}`} loading="lazy"/><span className="age-pill">{c.category}</span></div>
      <div className="course-content"><span className="category" style={{color:c.color}}>{c.level}</span><h3>{c.title}</h3><div className="program-facts"><span>Từ {c.ageMin} tuổi</span><span>{c.sessions} buổi học</span></div><p>{c.skills.join(' · ')}</p><button className="course-link" onClick={() => setSelected(c.id)} aria-expanded={selected === c.id}>Khám phá chương trình <ArrowRight size={16}/></button></div>
    </article>)}</div>
    {program && <CourseDetails course={program} classes={classes} onClose={()=>setSelected(null)}/>}
  </div></section>;
}

export function Method() {
  return <section className="section method" id="method"><div className="shell method-grid">
    <div className="method-image"><img src="https://images.unsplash.com/photo-1549490349-8643362247b5?auto=format&fit=crop&w=1000&q=90" alt="Không gian màu sắc và thử nghiệm hội họa" loading="lazy"/><div className="mini-card"><b>Ý tưởng của con</b><span>là điểm bắt đầu của mỗi tác phẩm</span></div></div>
    <div className="method-copy"><span className="kicker">CÁCH ARTORA ĐỒNG HÀNH</span><h2>Học kỹ thuật.<br/>Giữ nguyên trí tưởng tượng.</h2><p>Giáo viên không vẽ thay hay đưa một “đáp án đẹp”. Con được hướng dẫn từng kỹ thuật nền tảng, rồi tự chọn câu chuyện, màu sắc và cách thể hiện của mình.</p>
      <div className="method-list">{[
        ['Quan sát & khám phá', 'Khởi động bằng câu chuyện, vật thật và trải nghiệm giác quan.'],
        ['Thử nghiệm chất liệu', 'Khám phá màu, cọ, đất sét và kỹ thuật phù hợp với lộ trình học.'],
        ['Sáng tạo & phản hồi', 'Hoàn thiện tác phẩm và chia sẻ ý tưởng bằng ngôn ngữ của con.']
      ].map(([title, text], i) => <div key={title}><span>{String(i + 1).padStart(2, '0')}</span><div><b>{title}</b><p>{text}</p></div></div>)}</div>
    </div>
  </div></section>;
}

export function Gallery() {
  const photos = ['photo-1577083288073-40892c0860a4', 'photo-1547891654-e66ed7ebb968', 'photo-1550859492-d5da9d8e45f3', 'photo-1541961017774-22349e4a1262'];
  return <section className="section gallery" id="gallery"><div className="shell"><div className="gallery-head"><div><span className="kicker">GÓC SÁNG TẠO</span><h2>Mỗi gam màu,<br/>một cách kể chuyện.</h2></div><a href="#programs">Khám phá chương trình <ArrowRight size={17}/></a></div>
    <div className="gallery-grid">{photos.map((photo, i) => <figure key={photo} className={`g${i + 1}`}><img src={`https://images.unsplash.com/${photo}?auto=format&fit=crop&w=900&q=85`} loading="lazy" alt={['Cảm hứng khu vườn nhiều màu sắc', 'Bố cục màu sắc và hình khối', 'Thử nghiệm chất liệu hội họa', 'Tác phẩm minh họa nét vẽ tự do'][i]}/><figcaption>{['Màu của khu vườn', 'Thành phố trong mơ', 'Chuyện của biển', 'Những người bạn nhỏ'][i]}<small>Cảm hứng sáng tạo · Ảnh minh họa</small></figcaption></figure>)}</div>
  </div></section>;
}

export function StudioStory() {
  return <section className="section testimonials" id="about"><div className="shell"><span className="kicker">ĐIỀU ARTORA HƯỚNG ĐẾN</span><div className="quote">Không chỉ là một bức tranh đẹp.<br/>Là sự tự tin khi con kể về thế giới của mình.</div><div className="story-values"><span><Palette size={19}/> Tôn trọng nét riêng</span><span><BookOpen size={19}/> Đồng hành từng bước</span><span><ArrowRight size={19}/> Tiến bộ từ trải nghiệm</span></div><p className="story-description">Một không gian mỹ thuật để quan sát, thử nghiệm và thể hiện ý tưởng. Những điều học được cùng màu sắc sẽ theo con ra khỏi lớp học.</p></div></section>;
}

export function Questions() {
  return <section className="section public-faq" id="faq"><div className="shell faq-grid"><div><span className="kicker">TRƯỚC KHI BẮT ĐẦU</span><h2>Có thể bạn<br/>đang muốn biết.</h2><p>Khám phá Artora trước, chọn hành trình khi bạn sẵn sàng.</p><a className="text-link" href="#contact">Thông tin liên hệ <ArrowRight size={17}/></a></div><div>{[
    ['Chưa đăng nhập có xem được thông tin không?', 'Bạn có thể xem giới thiệu, chương trình, phương pháp, góc sáng tạo, học phí của khóa công bố và lịch lớp mà không cần đăng nhập. Tài khoản được dùng khi quản lý hồ sơ, đăng ký lớp và theo dõi học tập.'],
    ['Chưa từng học vẽ có thể bắt đầu không?', 'Trung tâm tư vấn và đánh giá đầu vào để tìm hướng học phù hợp. Các điều kiện cụ thể được công bố cùng khóa học; không cần tự chọn trình độ thay cho đánh giá của giáo viên.'],
    ['Làm sao biết học phí và lớp còn chỗ?', 'Xem mục khóa được công bố và lịch khai giảng. Mỗi lớp có học phí áp dụng và số chỗ còn lại. Trung tâm kiểm tra chỗ trống một lần nữa khi lập phiếu đăng ký.'],
    ['Một tài khoản có thể đăng ký cho nhiều học viên không?', 'Có. Người đại diện có thể tạo nhiều hồ sơ học viên trong tài khoản. Trung tâm cần xác minh hồ sơ và quyền đại diện trước khi ghi danh.'],
    ['Nếu nghỉ học hoặc muốn đổi lớp thì sao?', 'Bạn gửi thông báo hoặc đề nghị qua khu vực thành viên. Trung tâm xem xét theo quy định của khóa; việc học bù, chuyển lớp và hoàn tiền cần được xác nhận.']
  ].map(([title, text]) => <details key={title}><summary>{title}<ChevronDown size={17}/></summary><p>{text}</p></details>)}</div></div></section>;
}

export function Contact() {
  return <section className="section contact-section" id="contact"><div className="shell"><div className="section-head"><div><span className="kicker">GẶP ARTORA</span><h2>Bắt đầu bằng<br/>một cuộc trò chuyện.</h2></div><p>Cùng tìm lộ trình phù hợp trước khi chọn lớp và chuẩn bị cho buổi học đầu tiên.</p></div><div className="contact-grid"><article><span>GHÉ THĂM STUDIO</span><h3>36 Tân Thắng</h3><p>Tây Thạnh, TP.HCM</p></article><article><span>GỌI ĐỂ TƯ VẤN</span><a href="tel:0901676782">0901 676 782</a><p>Trao đổi về chương trình và lịch học.</p></article><article><span>GỬI LỜI NHẮN</span><a href="mailto:hello@artora.edu.vn">hello@artora.edu.vn</a><p>Thông tin liên hệ theo nội dung giới thiệu của studio.</p></article></div></div></section>;
}

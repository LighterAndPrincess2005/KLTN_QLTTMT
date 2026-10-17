import { useState } from 'react';
import { ArrowRight, BookOpen, ChevronDown, GraduationCap, Paintbrush } from 'lucide-react';
import type { ArtClass, Course } from '../types';
import {courseHref,pageHref,courseClassesHref} from '../navigation';
import {Pagination} from './CatalogTools';
import {PromotionHighlight} from './PromotionHighlight';

export function IntroPrograms({courses,classes}:{courses:Course[];classes:ArtClass[]}) {
  const [category, setCategory] = useState('Tất cả');
  const [page,setPage]=useState(1);
  const filtered=courses.filter(c=>category==='Tất cả'||c.category===category);
  const count=Math.ceil(filtered.length/6);
  const current=Math.min(page,count||1);
  return <section className="section programs" id="programs"><div className="shell">
    <div className="program-spotlight"><div className="spotlight-copy"><span className="spotlight-label"><img className="inline-brand" src="/bui/logo-user.jpg" alt=""/> KHÁM PHÁ CHƯƠNG TRÌNH</span><h2>Từ nét vẽ đầu tiên.<br/><span>Đến thế giới của riêng con.</span></h2><p>Mỗi chất liệu mở ra một cách sáng tạo. Tìm chương trình vừa với độ tuổi và hành trình của con.</p><a href="#program-selection" className="spotlight-button">Tìm khóa học cho con <ArrowRight size={18}/></a></div><div className="spotlight-art" aria-hidden="true"><img className="spotlight-brand" src="/bui/logo-user.jpg" alt=""/><span className="art-note">Một nét vẽ nhỏ.<br/>Vô vàn ý tưởng lớn.</span></div><div className="spotlight-path"><span><b>01</b> Khám phá chất liệu</span><span><b>02</b> Chọn hướng học</span><span><b>03</b> Tìm lớp phù hợp</span></div></div>
    <div className="program-selection-head" id="program-selection"><div><span className="kicker">CHƯƠNG TRÌNH HỌC</span><h3>Con sẽ bắt đầu từ đâu?</h3></div><p>Xem học phí, lộ trình và chất liệu; chọn khóa để khám phá chi tiết.</p></div>
    <div className="filter-row" aria-label="Nhóm chương trình">{['Tất cả', ...new Set(courses.map(c=>c.category))].map(x => <button type="button" aria-pressed={category === x} className={category === x ? 'active' : ''} key={x} onClick={() => {setCategory(x);setPage(1)}}>{x}</button>)}</div>
    <div className="course-grid">{filtered.slice((current-1)*6,current*6).map(c => <article className="course-card intro-course" key={c.id}>
      <div className="course-image"><img src={c.image} alt={`Khám phá chất liệu trong chương trình ${c.title}`} loading="lazy"/><span className="age-pill">{c.category}</span>{classes.find(cl=>cl.courseId===c.id&&cl.promotion)?.promotion&&<div className="program-promo"><PromotionHighlight promotion={classes.find(cl=>cl.courseId===c.id&&cl.promotion)!.promotion!}/></div>}</div>
      <div className="course-content"><span className="category">{c.level}</span><h3>{c.title}</h3><div className="program-facts"><span>Từ {c.ageMin} tuổi</span><span>{c.sessions} buổi học</span><span>{c.minutesPerSession} phút / buổi</span></div><div className="program-tuition"><span>Học phí khóa</span><strong>{c.tuitionFee.toLocaleString('vi-VN')}đ</strong></div><p>{c.skills.join(' · ')}</p><a className="course-link" href={courseHref(c.id)}>Khám phá chương trình <ArrowRight size={16}/></a><a className="btn btn-primary course-register-link" href={courseClassesHref(c.id)}>Đăng ký khóa học <ArrowRight size={16}/></a></div>
    </article>)}</div>
    <Pagination page={current} count={count} onChange={setPage}/>
  </div></section>;
}

export function Method() {
  return <section className="section method" id="method"><div className="shell method-grid">
    <div className="method-image"><img src="/bui/teacher.webp" alt="Giáo viên Mỹ Thuật Bụi hướng dẫn học viên thực hành" loading="lazy"/><div className="mini-card"><b>Ý tưởng của con</b><span>là điểm bắt đầu của mỗi tác phẩm</span></div></div>
    <div className="method-copy"><span className="kicker">CÁCH Mỹ Thuật Bụi ĐỒNG HÀNH</span><h2>Học kỹ thuật.<br/>Giữ nguyên trí tưởng tượng.</h2><p>Giáo viên không vẽ thay hay đưa một “đáp án đẹp”. Con được hướng dẫn từng kỹ thuật nền tảng, rồi tự chọn câu chuyện, màu sắc và cách thể hiện của mình.</p>
      <div className="method-list">{[
        ['Quan sát & khám phá', 'Khởi động bằng câu chuyện, vật thật và trải nghiệm giác quan.'],
        ['Thử nghiệm chất liệu', 'Khám phá màu, cọ, đất sét và kỹ thuật phù hợp với lộ trình học.'],
        ['Sáng tạo & phản hồi', 'Hoàn thiện tác phẩm và chia sẻ ý tưởng bằng ngôn ngữ của con.']
      ].map(([title, text], i) => <div key={title}><span>{String(i + 1).padStart(2, '0')}</span><div><b>{title}</b><p>{text}</p></div></div>)}</div>
    </div>
  </div></section>;
}

export function Gallery() {
  const artworks = [
    {image:'/bui/student-watercolor-4.jpg',title:'Chút xanh trên cành',medium:'Màu nước · bài tốt nghiệp',alt:'Tác phẩm học viên Bụi vẽ chim xanh trên cành cây',source:'https://mythuatbui.edu.vn/hcm-bai-tot-nghiep-khoa-mau-nuoc-08-thang-7/'},
    {image:'/bui/student-watercolor-5.jpg',title:'Một giỏ hồng dịu dàng',medium:'Màu nước · bài tốt nghiệp',alt:'Tác phẩm học viên Bụi vẽ giỏ hoa hồng',source:'https://mythuatbui.edu.vn/hcm-bai-tot-nghiep-khoa-mau-nuoc-08-thang-7/'},
    {image:'/bui/student-oil.jpg',title:'Sắc ngọt mùa hè',medium:'Sơn dầu · bài tốt nghiệp',alt:'Tác phẩm học viên Bụi vẽ kem trái cây trên nền xanh',source:'https://mythuatbui.edu.vn/hcm-bai-tot-nghiep-khoa-son-dau-can-ban-01-thang-7/'},
    {image:'/bui/student-sketch.jpg',title:'Những người bạn nhỏ',medium:'Bài tốt nghiệp · bộ bài Ký Họa 33',alt:'Tác phẩm những chú mèo trong bộ bài tốt nghiệp được Bụi công bố',source:'https://mythuatbui.edu.vn/hcm-bai-tot-nghie%CC%A3p-khoa-ki-ho%CC%A3a-33-tran-phu-thang-3/'},
  ];
  return <section className="section gallery student-gallery" id="gallery"><div className="shell"><div className="gallery-head"><div><span className="kicker">THÀNH PHẨM HỌC VIÊN</span><h2>Ý tưởng nhỏ.<br/><em>Thành phẩm đầy sắc màu.</em></h2><p className="gallery-intro">Từ quan sát đến phối màu, từ một nét phác đến bức tranh hoàn chỉnh — mỗi bài thực hành là một dịp để thể hiện ý tưởng theo cách riêng.</p></div><a href={pageHref('/khoa-hoc')}>Tìm hành trình sáng tạo <ArrowRight size={17}/></a></div>
    <div className="student-art-grid">{artworks.map((artwork, i) => <figure key={artwork.image} className={`student-art student-art-${i + 1}`}><div className="artwork-mat"><img src={artwork.image} loading="lazy" alt={artwork.alt}/><span className="artwork-number" aria-hidden="true">0{i+1}</span></div><figcaption><span>{artwork.medium}</span><h3>{artwork.title}</h3><a className="artwork-source" href={artwork.source} target="_blank" rel="noreferrer">Xem bài gốc tại Bụi ↗</a></figcaption></figure>)}</div>
    <div className="gallery-footnote"><span>Ảnh tác phẩm từ website Mỹ Thuật Bụi; tiêu đề mô tả hình ảnh, không phải tên gốc tác phẩm.</span><a href="https://www.instagram.com/hocvecungbui/" target="_blank" rel="noreferrer">Theo dõi @hocvecungbui <ArrowRight size={17}/></a></div>
  </div></section>;
}

export function StudioStory() {
  return <section className="section studio-introduction" id="about"><div className="shell introduction-grid"><div className="introduction-copy"><span className="kicker">GIỚI THIỆU TRUNG TÂM · MỸ THUẬT BỤI</span><h2>Một nơi để học vẽ.<br/><em>Và lớn lên cùng sáng tạo.</em></h2><p>Mỹ Thuật Bụi là không gian học mỹ thuật dành cho những bạn nhỏ thích khám phá. Từ nét vẽ đầu tiên đến những bài thực hành nhiều chất liệu, con được hướng dẫn kỹ thuật nền tảng và khuyến khích thể hiện ý tưởng của mình.</p><p>Chúng mình tin rằng điều đáng nhớ sau mỗi buổi học không chỉ là một bức tranh, mà còn là niềm vui thử một điều mới và sự tự tin khi kể câu chuyện của riêng con.</p><div className="introduction-cta"><span className="introduction-cta-label">HỌC THEO CÁCH CỦA BẠN</span><p>Từ quan sát đến thực hành, Bụi đồng hành trong từng nét vẽ.</p><a className="introduction-cta-button" href={pageHref('/phuong-phap')}>Khám phá cách Bụi đồng hành <ArrowRight size={19}/></a></div></div><div className="introduction-values"><span className="introduction-note">ĐIỀU MỸ THUẬT BỤI HƯỚNG ĐẾN</span>{[
    {Icon:Paintbrush,title:'Tôn trọng nét riêng',text:'Quan sát, lựa chọn màu sắc và tự kể câu chuyện qua tác phẩm.'},
    {Icon:BookOpen,title:'Đồng hành từng bước',text:'Lộ trình học từ nền tảng, hướng dẫn kỹ thuật theo chương trình.'},
    {Icon:GraduationCap,title:'Nhìn thấy sự tiến bộ',text:'Theo dõi bài thực hành, điểm danh và nhận xét từ giáo viên.'},
  ].map(({Icon,title,text})=><article key={title}><span><Icon size={24}/></span><div><h3>{title}</h3><p>{text}</p></div></article>)}</div></div></section>;
}

export function Questions() {
  return <section className="section public-faq" id="faq"><div className="shell faq-grid"><div><span className="kicker">TRƯỚC KHI BẮT ĐẦU</span><h2>Có thể bạn<br/>đang muốn biết.</h2><p>Khám phá Mỹ Thuật Bụi trước, chọn hành trình khi bạn sẵn sàng.</p><a className="text-link" href={pageHref('/#consultation')}>Hỏi Bụi điều bạn cần <ArrowRight size={17}/></a></div><div>{[
    ['Chưa đăng nhập có xem được thông tin không?', 'Bạn có thể xem giới thiệu, chương trình, phương pháp, góc sáng tạo, học phí của khóa công bố và lịch lớp mà không cần đăng nhập. Tài khoản được dùng khi quản lý hồ sơ, đăng ký lớp và theo dõi học tập.'],
    ['Chưa từng học vẽ có thể bắt đầu không?', 'Trung tâm tư vấn và đánh giá đầu vào để tìm hướng học phù hợp. Các điều kiện cụ thể được công bố cùng khóa học; không cần tự chọn trình độ thay cho đánh giá của giáo viên.'],
    ['Làm sao biết học phí và lớp còn chỗ?', 'Xem mục khóa được công bố và lịch khai giảng. Mỗi lớp có học phí áp dụng và số chỗ còn lại. Trung tâm kiểm tra chỗ trống một lần nữa khi lập phiếu đăng ký.'],
    ['Một tài khoản có thể đăng ký cho nhiều học viên không?', 'Có. Người đại diện có thể tạo nhiều hồ sơ học viên trong tài khoản. Bạn điền hồ sơ và chọn quan hệ đại diện để tiếp tục ghi danh.'],
    ['Nếu nghỉ học hoặc muốn đổi lớp thì sao?', 'Bạn gửi thông báo hoặc đề nghị qua khu vực thành viên. Trung tâm xem xét theo quy định của khóa; việc học bù, chuyển lớp và hoàn tiền cần được xác nhận.']
  ].map(([title, text]) => <details key={title}><summary>{title}<ChevronDown size={17}/></summary><p>{text}</p></details>)}</div></div></section>;
}

export function Contact() {
  return <section className="section contact-section" id="contact"><div className="shell"><div className="section-head"><div><span className="kicker">GẶP Mỹ Thuật Bụi</span><h2>Bắt đầu bằng<br/>một cuộc trò chuyện.</h2></div><p>Cùng tìm lộ trình phù hợp trước khi chọn lớp và chuẩn bị cho buổi học đầu tiên.</p></div><div className="contact-grid"><article><span>GHÉ THĂM STUDIO</span><h3>36 Tân Thắng</h3><p>Tây Thạnh, TP.HCM</p></article><article><span>GỌI ĐỂ TƯ VẤN</span><a href="tel:0901676782">0901 676 782</a><p>Trao đổi về chương trình và lịch học.</p></article><article><span>GỬI LỜI NHẮN</span><a href="mailto:info@mythuatbui.edu.vn">info@mythuatbui.edu.vn</a><p>Thông tin liên hệ theo nội dung giới thiệu của studio.</p></article></div></div></section>;
}

import {useEffect,useRef,useState} from 'react';
import {publicCourseReviews,courseReviewImage,type PublicCourseReview} from '../services/artCenterService';
import {pageHref} from '../navigation';
import { ArrowLeft, ArrowRight } from 'lucide-react';

const reviews = [
  {
    name: 'Trần Yến Lan',
    image: '/testimonials/tran-yen-lan.jpg',
    comment: 'Mỗi lần đi học về là em nào cũng mang theo một bức tranh ngộ nghĩnh dễ cưng mà mấy thầy cô đã tỉ mỉ chỉ dẫn cho. Chỗ này vừa có giá vẽ, dụng cụ, vừa học vừa được nghe nhạc nữa. Ở đây rất là thoải mái luôn.',
  },
  {
    name: 'Hương Giang',
    image: '/testimonials/huong-giang.jpg',
    comment: 'Các thầy cô còn giảng dạy rất kỹ. Mỗi ngày qua ngày, em càng ngày càng tiến bộ hơn và mỗi ngày em đều có thể đem một bức tranh về khoe mẹ. Môi trường học vui vẻ, sinh động, cơ sở vật chất đầy đủ.',
  },
  {
    name: 'Bảo Châu',
    image: '/testimonials/bao-chau.jpg',
    comment: '… vừa học vừa chơi rất thú vị như: vẽ màu trên ly thủy tinh, trên heo đất, trên áo, làm lồng đèn, làm hộp bút, …',
  },
];

export function StudentReviews() {
  const [published,setPublished]=useState<PublicCourseReview[]>([]);
  useEffect(()=>{let active=true;publicCourseReviews().then(items=>{if(active)setPublished(items)}).catch(()=>{});return()=>{active=false}},[]);

  const track=useRef<HTMLDivElement>(null);
  const [page,setPage]=useState(0);
  const pageSize=3;
  const items=published.length?published.map(review=>({
    ...review, anh:review.anh?courseReviewImage(review.anh):null, name:'Học viên Mỹ Thuật Bụi',
  })):reviews.map((review,index)=>({
    id:index, anh:review.image, name:review.name, noiDung:review.comment, soSao:null, tenKhoa:'Chia sẻ về trải nghiệm học mỹ thuật',
  }));
  const pages=Array.from({length:Math.ceil(items.length/pageSize)},(_,index)=>items.slice(index*pageSize,(index+1)*pageSize));
  const move=(index:number)=>{
    const element=track.current;
    if(!element)return;
    element.scrollTo({left:index*element.clientWidth,behavior:window.matchMedia('(prefers-reduced-motion: reduce)').matches?'auto':'smooth'});
  };

  return <section className="section student-reviews" id="reviews" aria-labelledby="reviews-title">
    <div className="shell reviews-paper">
      <div className="reviews-heading">
        <span className="kicker">ĐÁNH GIÁ & CẢM NHẬN HỌC VIÊN</span>
        <h2 id="reviews-title">Những câu chuyện<br/><em>sau một buổi học.</em></h2>
        {!published.length&&<p>Cảm nhận tham khảo</p>}
      </div>
      {pages.length>0&&<div className="reviews-track" ref={track} aria-label="Các nhóm đánh giá"
        onScroll={event=>{const element=event.currentTarget;if(element.clientWidth)setPage(Math.round(element.scrollLeft/element.clientWidth));}}>
        {pages.map(group=><div className="reviews-grid" key={group[0].id}>
        {group.map(review=><figure className="student-review" key={review.id}>
          {review.anh&&<div className="review-portrait"><img src={review.anh} alt={published.length?"Thành phẩm học viên":review.name} width="300" height="300" loading="lazy"/></div>}
          {review.soSao!==null&&<p aria-label={`${review.soSao} sao`} className="review-name">{'★'.repeat(review.soSao)}</p>}
          <blockquote><p>{review.noiDung}</p></blockquote>
          <figcaption><span className="review-name">{review.name}</span><span className="review-role">{review.tenKhoa}</span></figcaption>
        </figure>)}
        </div>)}
      </div>}
      {pages.length>1&&<div className="reviews-pagination">
        <button type="button" className="btn btn-soft" onClick={()=>move(page-1)} disabled={page===0} aria-label="Nhóm đánh giá trước"><ArrowLeft size={18}/></button>
        <span aria-live="polite">{page+1} / {pages.length}</span>
        <button type="button" className="btn btn-soft" onClick={()=>move(page+1)} disabled={page>=pages.length-1} aria-label="Nhóm đánh giá tiếp theo"><ArrowRight size={18}/></button>
      </div>}
      <div className="reviews-footer"><p>Mỗi trải nghiệm đều là một dịp để lắng nghe.</p><a className="text-link" href={pageHref('/hoc-tap')}>Đánh giá khóa học <ArrowRight size={18}/></a></div>
    </div>
  </section>;
}

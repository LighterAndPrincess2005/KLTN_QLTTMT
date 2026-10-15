import { ArrowRight, Quote } from 'lucide-react';

// Portraits and excerpts supplied as a reference by the user.
// These examples are separate from private feedback in the member API.
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
  return <section className="section student-reviews" id="reviews" aria-labelledby="reviews-title">
    <div className="shell reviews-paper">
      <div className="reviews-heading">
        <span className="kicker">ĐÁNH GIÁ & CẢM NHẬN HỌC VIÊN</span>
        <h2 id="reviews-title">Những câu chuyện<br/><em>sau một buổi học.</em></h2>
        <span className="reviews-sample">Ảnh và nhận xét minh họa</span>
      </div>
      <div className="reviews-grid">
        {reviews.map(review => <figure className="student-review" key={review.name}>
          <div className="review-portrait">
            <img src={review.image} alt={`Ảnh tham khảo của ${review.name}`} width="300" height="300" loading="lazy"/>
            <Quote className="review-quote" size={30} aria-hidden="true"/>
          </div>
          <blockquote><p>{review.comment}</p></blockquote>
          <figcaption><span className="review-name">{review.name}</span><span className="review-role">Chia sẻ về trải nghiệm học mỹ thuật</span></figcaption>
        </figure>)}
      </div>
      <div className="reviews-footer"><p>Mỗi trải nghiệm đều là một dịp để lắng nghe.</p><a className="text-link" href="#member">Gửi phản hồi về lớp học <ArrowRight size={18}/></a></div>
    </div>
  </section>;
}

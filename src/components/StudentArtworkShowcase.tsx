import {ArrowUpRight} from 'lucide-react';

const source='https://mythuatbui.edu.vn/hcm-bai-tot-nghiep-khoa-mau-nuoc-08-thang-7/';
const works=[
 {image:'/bui/student-watercolor-1.jpg',alt:'Bài màu nước vẽ đĩa bánh, trái cây và quả mọng',caption:'Quan sát từ những điều quen thuộc'},
 {image:'/bui/student-watercolor-2.jpg',alt:'Bài màu nước vẽ chú chim đậu trên cành hoa',caption:'Thiên nhiên qua những lớp màu'},
 {image:'/bui/student-watercolor-3.jpg',alt:'Bài màu nước vẽ bánh mì, dâu và quả việt quất',caption:'Chút ngọt ngào trong từng nét vẽ'}
];

export function StudentArtworkShowcase(){
 return <section className="section student-art-showcase" aria-labelledby="student-art-title"><div className="shell">
  <div className="student-art-heading"><div><span className="kicker">TỪ LỚP HỌC ĐẾN TÁC PHẨM</span><h2 id="student-art-title">Mỗi nét vẽ,<br/>một dấu ấn riêng.</h2></div><p>Nhìn lại những thành phẩm của học viên: từ một mẫu vật gần gũi đến bức tranh mang cách cảm nhận của chính người vẽ.</p></div>
  <div className="student-art-showcase-grid">{works.map((work,i)=><figure key={work.image}><a href={source} target="_blank" rel="noreferrer" aria-label={'Xem bộ bài tốt nghiệp màu nước trên website Bụi: '+work.caption}><div className="student-art-frame"><img src={work.image} alt={work.alt} loading="lazy"/><span><ArrowUpRight size={20}/></span></div></a><figcaption><small>MÀU NƯỚC · BÀI TỐT NGHIỆP</small><h3>{work.caption}</h3><span>0{i+1} / Góc thành phẩm</span></figcaption></figure>)}</div>
  <div className="student-art-footer"><p>Ảnh tác phẩm học viên từ bài tốt nghiệp lớp Màu Nước 08, TP. Hồ Chí Minh, do Mỹ Thuật Bụi công bố. Chú thích trên trang mô tả hình ảnh, không phải tên gốc của tác phẩm.</p><a href={source} target="_blank" rel="noreferrer">Xem bộ tác phẩm tại Bụi <ArrowUpRight size={17}/></a></div>
 </div></section>;
}

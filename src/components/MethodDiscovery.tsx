import {useArticles} from '../services/websiteService';
import {pageHref} from '../navigation';
import {useState} from 'react';
import {ArrowUpRight,Check,Eye,Palette,Sparkles} from 'lucide-react';

const lenses=[
 {title:'Màu sắc',question:'Màu nào khiến bạn dừng mắt đầu tiên?',text:'Thử tìm những sắc ấm ở trái cây và sắc lạnh trong nền tranh. Nếu đổi nền sang màu đỏ, cảm giác của bạn có khác không?',colors:['#c78d3e','#8a9d8b','#e4dfca','#374b3e']},
 {title:'Bố cục',question:'Ánh mắt bạn đi qua những đâu?',text:'Theo dõi đường nhìn từ chiếc giỏ đến chai, đĩa và các trái táo. Thử che một góc tranh bằng tay: điều gì thay đổi?',colors:['#7c593d','#c4ad7e','#d8d2b6','#596543']},
 {title:'Ánh sáng',question:'Đâu là vùng sáng nhất trong tranh?',text:'Quan sát nếp khăn sáng và bóng đổ quanh các vật. Khi vẽ một quả táo, bạn sẽ giữ vùng nào sáng và đặt bóng ở đâu?',colors:['#f0e9d3','#b5baa3','#6b7662','#343f32']},
 {title:'Cảm xúc',question:'Bạn sẽ đặt tên gì cho khoảnh khắc này?',text:'Không cần tìm một đáp án đúng. Chọn ba từ mô tả cảm giác của bạn, rồi chia sẻ chi tiết trong tranh khiến bạn nghĩ như vậy.',colors:['#b88545','#84957b','#dad5ba','#726247']}
];
const materials=[
 {name:'Chì & ký họa',image:'/bui/sketch.jpg',tag:'Bắt đầu từ nét',text:'Khám phá đường nét, hình khối và sắc độ trước khi nghĩ đến màu sắc.',try:'Thử vẽ chiếc cốc chỉ bằng 10 nét.'},
 {name:'Màu nước',image:'/bui/watercolor.jpg',tag:'Nhẹ và trong',text:'Tận dụng độ trong của màu và khoảng trắng trên giấy để tạo cảm giác thoáng, nhẹ.',try:'Thử pha một màu thành ba mức đậm nhạt.'},
 {name:'Acrylic',image:'/bui/acrylic.jpg',tag:'Mảng màu cá tính',text:'Màu khô nhanh, phù hợp thử nghiệm các lớp màu và mảng hình rõ ràng.',try:'Thử kể một câu chuyện bằng ba mảng màu.'}
];
const exercises=[
 {name:'Một vật, nhiều cách nhìn',tool:'Một chiếc lá, bút chì và giấy',steps:['Quan sát chiếc lá trong một phút; tìm đường gân và phần viền đặc biệt.','Phác hình lớn, rồi thêm năm chi tiết bạn vừa nhìn thấy.','Đổi hướng chiếc lá và vẽ lại bằng nét liền, không nhấc bút.']},
 {name:'Nhật ký bằng màu',tool:'Giấy và ba màu bạn đang có',steps:['Chọn ba màu diễn tả tâm trạng hôm nay.','Tạo các mảng lớn nhỏ, không cần vẽ thành một vật cụ thể.','Đặt tên cho bức tranh và viết một câu kể về nó.']},
 {name:'Bóng của chiếc cốc',tool:'Chiếc cốc, đèn bàn, bút chì và giấy',steps:['Đặt cốc dưới một nguồn sáng và quan sát bóng đổ.','Phác chiếc cốc bằng các hình đơn giản.','Chia bài vẽ thành ba vùng: sáng, trung gian và tối.']}
];
export function MethodDiscovery(){
 const {items:articles,loading:readingLoading,error:readingError}=useArticles();
 const readings=articles.slice(0,3).map(a=>({title:a.title,text:a.intro,image:a.image,href:'/bai-viet/'+a.slug,tag:a.category}));
 const [lens,setLens]=useState(0);
 const [exercise,setExercise]=useState(0);
 const [checked,setChecked]=useState<number[]>([]);
 const active=lenses[lens];
 return <div className="method-discovery">
  <section className="section md-observe"><div className="shell">
   <div className="md-heading"><span className="kicker">NHÌN CHẬM LẠI MỘT CHÚT</span><h2>Một bức tranh.<br/>Nhiều điều để khám phá.</h2><p>Không cần biết tên trường phái để bắt đầu thưởng thức mỹ thuật. Chọn một góc nhìn, ngắm tranh và thử trả lời theo cách của bạn.</p></div>
   <div className="md-observe-grid"><figure><img src="/bui/stilllife-learning.jpg" alt="Tranh tĩnh vật Giỏ táo của Paul Cézanne với trái cây, chai và khăn trên bàn" loading="lazy"/><figcaption>Paul Cézanne · Giỏ táo (The Basket of Apples)<a href={pageHref('/bai-viet/ngam-tranh-tinh-vat')}>Khám phá bức tranh <ArrowUpRight size={14}/></a></figcaption></figure>
   <div className="md-lens-card"><div className="md-label"><Eye size={18}/> GÓC NHÌN CỦA BẠN</div><div className="md-lens-tabs" role="tablist" aria-label="Góc quan sát tranh">{lenses.map((item,i)=><button key={item.title} id={'lens-tab-'+i} role="tab" aria-selected={lens===i} aria-controls="lens-panel" onClick={()=>setLens(i)}>{item.title}</button>)}</div><div id="lens-panel" role="tabpanel" aria-labelledby={'lens-tab-'+lens}><h3>{active.question}</h3><p>{active.text}</p><div className="md-swatches" aria-label="Bảng màu gợi ý">{active.colors.map(color=><span key={color} style={{background:color}}/>)}</div><small>Gợi ý tự quan sát, không phải đáp án hay bài kiểm tra.</small></div></div></div>
  </div></section>
  <section className="section md-materials"><div className="shell"><div className="md-heading"><span className="kicker">CHẠM VÀO CHẤT LIỆU</span><h2>Mỗi chất liệu, một cách kể chuyện.</h2><p>Hiểu một chút về công cụ để chọn cách thể hiện phù hợp với ý tưởng của mình.</p></div><div className="md-material-grid">{materials.map(item=><article key={item.name}><img src={item.image} alt={item.name+' — hình ảnh chất liệu tham khảo'} loading="lazy"/><div><span>{item.tag}</span><h3>{item.name}</h3><p>{item.text}</p><div className="md-try"><Palette size={19}/>{item.try}</div></div></article>)}</div><div className="md-source md-source-action"><span>Chưa biết nên bắt đầu với chất liệu nào?</span><a href={pageHref('/bai-viet/chon-chat-lieu-ve')}>Xem cách chọn chất liệu <ArrowUpRight size={17} aria-hidden="true"/></a><small>Hình ảnh minh họa theo chất liệu.</small></div></div></section>
  <section className="section md-practice"><div className="shell md-practice-grid"><div><span className="kicker">RỜI MÀN HÌNH, THỬ VẼ NÀO</span><h2>Dành 5 phút<br/>cho một nét mới.</h2><p>Không cần bộ họa cụ cầu kỳ. Chọn một thử thách nhỏ và bắt đầu với những gì bạn có.</p><div className="md-exercise-options" role="group" aria-label="Chọn bài thực hành">{exercises.map((item,i)=><button key={item.name} aria-pressed={exercise===i} onClick={()=>{setExercise(i);setChecked([]);}}><span>0{i+1}</span>{item.name}<ArrowUpRight size={17}/></button>)}</div></div><div className="md-exercise-card"><div className="md-label"><Sparkles size={18}/> SỔ TAY THỰC HÀNH</div><h3>{exercises[exercise].name}</h3><p>Chuẩn bị: {exercises[exercise].tool}.</p><div className="md-checklist">{exercises[exercise].steps.map((step,i)=><label key={exercise+'-'+i}><input type="checkbox" checked={checked.includes(i)} onChange={()=>setChecked(current=>current.includes(i)?current.filter(n=>n!==i):[...current,i])}/><span>{step}</span></label>)}</div><div className="md-progress" aria-live="polite">{checked.length===3?<><Check size={18}/> Đã hoàn thành! Thử chia sẻ điều bạn thích nhất trong bài vẽ.</>:<>{checked.length}/3 bước · Cứ thử, không cần vẽ thật hoàn hảo.</>}</div><small>Bài tập gợi ý tự trải nghiệm; không chấm điểm hoặc lưu vào hồ sơ học viên.</small></div></div></section>
  <section className="section md-reading"><div className="shell"><div className="md-reading-head"><div><span className="kicker">ĐỌC MỘT CHÚT, THẤY NHIỀU HƠN</span><h2>Góc thưởng thức mỹ thuật.</h2></div><a href={pageHref('/bai-viet')}>Xem tất cả bài viết <ArrowUpRight size={18}/></a></div>{readingLoading&&<p role="status">Đang tải bài viết…</p>}{readingError&&<p className="error-note" role="alert">{readingError}</p>}<div className="md-reading-grid">{readings.map(item=><a className="md-reading-card" href={pageHref(item.href)} key={item.href}><div className="md-reading-image"><img src={item.image} alt={item.title} loading="lazy"/><span><ArrowUpRight size={21}/></span></div><div><small>{item.tag}</small><h3>{item.title}</h3><p>{item.text}</p><span className="md-read-link">Đọc tiếp →</span></div></a>)}</div><p className="md-source">Chọn một chủ đề yêu thích và tiếp tục khám phá trong góc đọc.</p></div></section>
 </div>;
}

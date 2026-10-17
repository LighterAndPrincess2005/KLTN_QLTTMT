import {useState} from 'react';
import {ArrowRight,Check,Mail,MessageCircle} from 'lucide-react';

type ConsultationDraft={name:string;email:string;note:string};
const empty:ConsultationDraft={name:'',email:'',note:''};
const key='bui.consultation.draft';

export function ConsultationCorner(){
 const [draft,setDraft]=useState<ConsultationDraft>(()=>{
   try{const cached=JSON.parse(sessionStorage.getItem(key)||'null');return cached&&typeof cached.name==='string'&&typeof cached.email==='string'&&typeof cached.note==='string'?cached:empty}catch{return empty}
 });
 const [saved,setSaved]=useState(false),[error,setError]=useState('');
 const update=(field:keyof ConsultationDraft,value:string)=>{setDraft(current=>({...current,[field]:value}));setSaved(false);setError('')};
 return <section className="consultation-corner" id="consultation" aria-labelledby="consultation-title">
   <div className="shell consultation-grid">
     <div className="consultation-copy"><span className="consultation-kicker"><MessageCircle size={18}/> MÌNH CÙNG TRÒ CHUYỆN NHÉ</span><h2 id="consultation-title">Góc tư vấn<span>Một chút chia sẻ.<br/>Một hướng học phù hợp.</span></h2><p>Bạn muốn bắt đầu học vẽ, tìm lớp cho con hay khám phá một chất liệu mới? Hãy để lại điều bạn đang quan tâm.</p><div className="consultation-points"><span><Check size={17}/> Tìm chương trình phù hợp</span><span><Check size={17}/> Trao đổi lịch học và học phí</span></div><small className="consultation-credit">Không gian lớp học tại Mỹ Thuật Bụi</small></div>
     <form className="consultation-form" onSubmit={event=>{
       event.preventDefault();setError('');setSaved(false);
       if(!draft.name.trim()||!draft.note.trim()){setError('Bạn vui lòng nhập họ tên và nội dung cần tư vấn.');return}
       try{sessionStorage.setItem(key,JSON.stringify({name:draft.name.trim(),email:draft.email.trim(),note:draft.note.trim()}));setSaved(true)}catch{setError('Chưa lưu được thông tin trên thiết bị. Vui lòng thử lại.')}
     }}>
       <div className="consultation-form-heading"><span><Mail size={21}/></span><div><h3>Bạn cần Bụi tư vấn điều gì?</h3><p>Điền thông tin để chuẩn bị lời nhắn của bạn.</p></div></div>
       <label className="field"><span>Họ và tên</span><input name="consultationName" autoComplete="name" value={draft.name} onChange={e=>update('name',e.target.value)} required maxLength={150} placeholder="Tên của bạn"/></label>
       <label className="field"><span>Email</span><input name="consultationEmail" type="email" autoComplete="email" value={draft.email} onChange={e=>update('email',e.target.value)} required maxLength={254} placeholder="ban@email.com"/></label>
       <label className="field"><span>Ghi chú / Nội dung cần tư vấn</span><textarea name="consultationNote" value={draft.note} onChange={e=>update('note',e.target.value)} required maxLength={2000} rows={4} placeholder="Ví dụ: Mình muốn tìm lớp màu nước cuối tuần cho người mới bắt đầu…"/></label>
       {error&&<p className="error-note" role="alert">{error}</p>}
       {saved&&<p className="consultation-saved" role="status"><Check size={18}/> Đã lưu bản nháp trên thiết bị. Nội dung chưa được gửi đến trung tâm.</p>}
       <button type="submit" className="btn btn-primary full">Lưu thông tin tư vấn <ArrowRight size={18}/></button>
       <p className="consultation-form-note">Biểu mẫu hiện lưu bản nháp trong phiên trình duyệt, chưa gửi email đến trung tâm.</p>
     </form>
   </div>
 </section>;
}

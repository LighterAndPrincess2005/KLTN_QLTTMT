import { useEffect, useState } from 'react';
import { request } from '../services/artCenterService';

type Review = {trangThai:string;ghiChuDuyet?:string};
const labels:Record<string,string> = {
  CHO_DUYET:'Đang chờ duyệt', DA_DUYET:'Đã duyệt công khai',
  RIENG_TU:'Đã gửi riêng cho trung tâm', TU_CHOI:'Trung tâm từ chối công bố', AN:'Đã ẩn khỏi trang chủ',
};

export function CourseReviewForm({profileId}:{profileId:number}) {
  const [review,setReview]=useState<Review|null>();
  const [error,setError]=useState('');
  const [busy,setBusy]=useState(false);
  const [attempt,setAttempt]=useState(0);
  useEffect(()=>{
    let active=true;
    request<{review:Review|null}>(`/api/danh-gia-khoa/cua-toi/${profileId}`)
      .then(data=>{if(active)setReview(data.review)})
      .catch(e=>{if(active)setError(e.message)});
    return ()=>{active=false};
  },[profileId,attempt]);
  return <>
    <h4>Đánh giá khóa học</h4>
    {error&&<p className="error-note" role="alert">{error}</p>}
    {review===undefined?(error?<button className="btn btn-soft" onClick={()=>{setError('');setAttempt(n=>n+1)}}>Tải lại</button>:<p role="status">Đang tải đánh giá…</p>)
      :review?<div className="learning-result" role="status"><b>{labels[review.trangThai]}</b>{review.ghiChuDuyet&&<p>{review.ghiChuDuyet}</p>}</div>
      :<form className="learning-form" onSubmit={async event=>{
        event.preventDefault();
        const data=new FormData(event.currentTarget);
        data.set('profileId',String(profileId));
        data.set('dongYCongKhai',String(data.has('dongYCongKhai')));
        const image=data.get('anh');
        if(image instanceof File&&!image.size)data.delete('anh');
        else if(image instanceof File&&image.size>5*1024*1024){setError('Ảnh tối đa 5 MB.');return}
        setBusy(true);setError('');
        try { setReview(await request<Review>('/api/danh-gia-khoa',data)); }
        catch(e){setError(e instanceof Error?e.message:'Chưa gửi được đánh giá.');}
        finally{setBusy(false);}
      }}>
        <label className="field"><span>Mức độ hài lòng</span><select name="soSao" required defaultValue=""><option value="" disabled>Chọn số sao</option>{[5,4,3,2,1].map(n=><option key={n} value={n}>{n} sao</option>)}</select></label>
        <label className="field"><span>Chia sẻ trải nghiệm</span><textarea name="noiDung" required maxLength={2000} rows={4}/></label>
        <label className="field"><span>Ảnh thành phẩm (không bắt buộc, PNG hoặc JPEG, tối đa 5 MB)</span><input name="anh" type="file" accept="image/jpeg,image/png"/></label>
        <label><input name="dongYCongKhai" type="checkbox"/> Tôi đồng ý để trung tâm công bố nhận xét và ảnh đính kèm sau khi duyệt.</label>
        <p className="learning-intro">Bạn có thể gửi riêng cho trung tâm. Khi công bố, trang chủ dùng tên chung «Học viên Mỹ Thuật Bụi». Vui lòng chọn ảnh thành phẩm không có thông tin cá nhân.</p>
        <button className="btn btn-primary" disabled={busy}>{busy?'Đang gửi…':'Gửi đánh giá'}</button>
      </form>}
  </>;
}

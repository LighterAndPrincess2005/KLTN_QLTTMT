import { Star } from 'lucide-react';
import { useEffect, useState } from 'react';
import { request } from '../services/artCenterService';

type Review = {trangThai:string;ghiChuDuyet?:string;coTheHienTen?:boolean;tenNguoiDangCongKhai?:string};
const labels:Record<string,string> = {
  CHO_DUYET:'Đang chờ duyệt', DA_DUYET:'Đã duyệt công khai',
  RIENG_TU:'Đã gửi riêng cho trung tâm', TU_CHOI:'Trung tâm từ chối công bố', AN:'Đã ẩn khỏi trang chủ',
};

export function CourseReviewForm({profileId}:{profileId:number}) {
  const [review,setReview]=useState<Review|null>();
  const [error,setError]=useState('');
  const [busy,setBusy]=useState(false);
  const [attempt,setAttempt]=useState(0);
  const [rating,setRating]=useState(0);
  const [publish,setPublish]=useState(false);
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
      :review?<div className="learning-result" role="status"><b>{labels[review.trangThai]}</b>{review.ghiChuDuyet&&<p>{review.ghiChuDuyet}</p>}
        {review.coTheHienTen&&<><p>Tên hiển thị: {review.tenNguoiDangCongKhai||'Học viên Mỹ Thuật Bụi'}</p>
          <button type="button" className="btn btn-soft" disabled={busy} onClick={async()=>{
            setBusy(true);setError('');
            try{setReview(await request<Review>(`/api/danh-gia-khoa/cua-toi/${profileId}/ten`,{hienTen:!review.tenNguoiDangCongKhai},'PUT'));}
            catch(e){setError(e instanceof Error?e.message:'Chưa cập nhật được tên hiển thị.');}
            finally{setBusy(false);}
          }}>{review.tenNguoiDangCongKhai?'Ẩn tên của tôi':'Hiện tên của tôi'}</button></>}
      </div>
      :<form className="learning-form" onSubmit={async event=>{
        event.preventDefault();
        const data=new FormData(event.currentTarget);
        data.set('profileId',String(profileId));
        data.set('dongYCongKhai',String(data.has('dongYCongKhai')));
        data.set('hienTen',String(data.has('hienTen')));
        const image=data.get('anh');
        if(image instanceof File&&!image.size)data.delete('anh');
        else if(image instanceof File&&image.size>5*1024*1024){setError('Ảnh tối đa 5 MB.');return}
        setBusy(true);setError('');
        try { setReview(await request<Review>('/api/danh-gia-khoa',data)); }
        catch(e){setError(e instanceof Error?e.message:'Chưa gửi được đánh giá.');}
        finally{setBusy(false);}
      }}>
        <fieldset className="course-rating" disabled={busy}>
          <legend>Mức độ hài lòng</legend>
          <div className="course-rating-stars">
            {[1,2,3,4,5].map(n=><label key={n} title={`${n} sao`}>
              <input type="radio" name="soSao" value={n} aria-label={`${n} sao`} required
                checked={rating===n} onChange={()=>setRating(n)}/>
              <Star size={25} aria-hidden="true" fill={n<=rating?'currentColor':'none'}/>
            </label>)}
          </div>
          <small>{rating?`Đã chọn ${rating}/5 sao`:'Bấm vào ngôi sao để đánh giá'}</small>
        </fieldset>
        <label className="field"><span>Chia sẻ trải nghiệm</span><textarea name="noiDung" required maxLength={2000} rows={4}/></label>
        <label className="field"><span>Ảnh thành phẩm (không bắt buộc, PNG hoặc JPEG, tối đa 5 MB)</span><input name="anh" type="file" accept="image/jpeg,image/png"/></label>
        <label><input name="dongYCongKhai" type="checkbox" checked={publish} onChange={event=>setPublish(event.target.checked)}/> Tôi đồng ý để trung tâm công bố nhận xét và ảnh đính kèm sau khi duyệt.</label>
        {publish&&<label><input name="hienTen" type="checkbox"/> Hiện tên tài khoản của tôi trên trang chủ.</label>}
        <p className="learning-intro">Bạn có thể gửi riêng cho trung tâm. Vui lòng chọn ảnh thành phẩm không có thông tin cá nhân.</p>
        <button className="btn btn-primary" disabled={busy}>{busy?'Đang gửi…':'Gửi đánh giá'}</button>
      </form>}
  </>;
}

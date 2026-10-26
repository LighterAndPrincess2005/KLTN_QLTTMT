import {useEffect,useState} from 'react';
import {Clock3,ChevronDown} from 'lucide-react';
import {dateTime,vnTime,type Enrollment} from '../services/artCenterService';
import {DebtSummary} from './DebtSummary';
import './pendingPayments.css';

export function holdTimeLabel(deadline:string,now:number){
 const milliseconds=vnTime(deadline).getTime()-now;
 if(!Number.isFinite(milliseconds))return 'Chưa tải được hạn giữ chỗ';
 if(milliseconds<=0)return 'Đã hết hạn giữ chỗ';
 const minutes=Math.ceil(milliseconds/60000);
 if(minutes<60)return `Còn ${minutes} phút để giữ chỗ`;
 const hours=Math.floor(minutes/60),rest=minutes%60;
 if(hours<24)return `Còn ${hours} giờ${rest?` ${rest} phút`:''} để giữ chỗ`;
 return `Còn ${Math.floor(hours/24)} ngày${hours%24?` ${hours%24} giờ`:''} để giữ chỗ`;
}

export function PendingPayments({enrollments,onRefresh}:{enrollments:Enrollment[];onRefresh:()=>void}){
 const [now,setNow]=useState(()=>Date.now());
 useEffect(()=>{const timer=window.setInterval(()=>setNow(Date.now()),1000);return()=>window.clearInterval(timer)},[]);
 const rows=enrollments.filter(row=>row.trangThai==='CHO_THANH_TOAN').sort((a,b)=>vnTime(a.hanGiuCho).getTime()-vnTime(b.hanGiuCho).getTime());
 if(!rows.length)return null;
 return <section className="study-pending-highlight" aria-labelledby="pending-payments-title">
  <div className="pending-payments-heading"><Clock3 size={24} aria-hidden="true"/><div><h2 id="pending-payments-title">Bạn có {rows.length} phiếu chờ thanh toán</h2><p>Lịch học cá nhân sẽ xuất hiện sau khi thanh toán đủ và được trung tâm xác nhận.</p></div></div>
  <div className="pending-payments-list">{rows.map(row=>{
   const expired=vnTime(row.hanGiuCho).getTime()<=now;
   return <article className="pending-payment-card" key={row.id}>
    <div className="pending-payment-title"><div><h3>{row.tenKhoa}</h3><p>{row.hocVien} · {row.tenLop}</p></div><span className="pending-payment-status">{expired?'Đã quá hạn giữ chỗ':'Chờ thanh toán'}</span></div>
    <p className="pending-payment-code">Mã đăng ký: {row.maDangKy}</p>
    <div className="pending-payment-deadline"><b><Clock3 size={17} aria-hidden="true"/>Hạn giữ chỗ: {dateTime(row.hanGiuCho)}</b><span>{holdTimeLabel(row.hanGiuCho,now)}</span></div>
    {expired?<div className="pending-payment-expired"><p>Phiếu đã qua hạn giữ chỗ. Vui lòng cập nhật trạng thái trước khi đăng ký lại hoặc liên hệ trung tâm nếu đã trả một phần. Không tiếp tục thanh toán theo phiếu đã hết hạn.</p><button type="button" className="btn btn-soft" onClick={onRefresh}>Cập nhật trạng thái</button></div>:
     <details className="pending-payment-details"><summary>Xem phiếu và hướng dẫn thanh toán <ChevronDown size={17} aria-hidden="true"/></summary><DebtSummary id={row.id}/></details>}
   </article>;
  })}</div>
 </section>;
}

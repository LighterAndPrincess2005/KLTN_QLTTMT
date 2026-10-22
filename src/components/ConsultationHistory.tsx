import {useEffect,useState} from 'react';
import {websiteApi,type AdviceDetail} from '../services/websiteService';
import {signedIn,dateTime} from '../services/artCenterService';
export const adviceStatus:Record<string,string>={MOI_GUI:'Mới gửi',DA_DOC:'Đã đọc',DANG_XU_LY:'Đang xử lý',DA_TRA_LOI:'Đã trả lời',DA_DONG:'Đã đóng'};
export function AdviceMessages({detail}:{detail:AdviceDetail}){return <div className="advice-thread"><p><b>{detail.maTraCuu}</b> · {adviceStatus[detail.trangThai]||detail.trangThai}</p>{detail.tinNhan.map(m=><article key={m.id} className={m.tuNhanVien?'staff-message':''}><b>{m.tuNhanVien?'Trung tâm':'Người gửi'}</b><small>{dateTime(m.guiLuc)}</small><p>{m.noiDung}</p></article>)}</div>}
export function ConsultationHistory(){
 const [code]=useState(''),[secret,setSecret]=useState(''),[detail,setDetail]=useState<AdviceDetail|null>(null),[ownId,setOwnId]=useState<number|null>(null),[error,setError]=useState(''),[busy,setBusy]=useState(false),[mine,setMine]=useState<{id:number;maTraCuu:string;chuDe:string;trangThai:string}[]>([]);
 useEffect(()=>{if(signedIn())void websiteApi.mine().then(setMine).catch(()=>{})},[]);
 if(!signedIn()||mine.length===0)return null;
 return <div className="consultation-history"><h3>Tư vấn của tôi</h3><p>Chọn cuộc trao đổi để xem phản hồi, không cần lưu mã hoặc khóa tra cứu.</p><div className="advice-own-list">{mine.map(x=><button key={x.id} type="button" onClick={async()=>{setError('');try{setDetail(await websiteApi.ownAdvice(x.id));setOwnId(x.id);setSecret('')}catch(e){setError(String(e))}}}>{x.chuDe} · {adviceStatus[x.trangThai]}</button>)}</div>
 {error&&<p className="error-note" role="alert">{error}</p>}{detail&&<><AdviceMessages detail={detail}/>{(secret||ownId)&&detail.trangThai!=='DA_DONG'&&<form onSubmit={async e=>{e.preventDefault();const form=e.currentTarget;setBusy(true);setError('');try{const text=String(new FormData(form).get('text'));if(ownId)await websiteApi.ownFollowup(ownId,text,detail.rowVersion);else await websiteApi.followup(code,secret,text);form.reset();setDetail(ownId?await websiteApi.ownAdvice(ownId):await websiteApi.lookup(code,secret))}catch(e){setError(e instanceof Error?e.message:'Chưa gửi được.')}finally{setBusy(false)}}}><label className="field"><span>Gửi thêm nội dung</span><textarea name="text" required maxLength={4000} rows={3}/></label><button className="btn btn-primary" disabled={busy}>Gửi bổ sung</button></form>}</>}
 </div>;
}

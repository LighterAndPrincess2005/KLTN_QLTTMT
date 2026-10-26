import {useState} from 'react';
import {artCenterService as api,clearSession,ApiError} from '../services/artCenterService';
import './passwordRecovery.css';

export function PasswordRecovery({onBack}:{onBack:()=>void}){
 const [id,setId]=useState(''),[busy,setBusy]=useState(false),[error,setError]=useState(''),[notice,setNotice]=useState(''),[done,setDone]=useState(false),[serviceUnavailable,setServiceUnavailable]=useState(false);
 const [newPassword,setNewPassword]=useState(''),[confirmPassword,setConfirmPassword]=useState(''),[focusedSecret,setFocusedSecret]=useState<'new'|'confirm'|null>(null);
 return <div className="modal-body password-recovery"><span className="kicker">KHÔI PHỤC TÀI KHOẢN</span><h2>{done?'Đã đặt lại mật khẩu':id?'Nhập mã khôi phục':'Quên mật khẩu'}</h2>
 <p>{done?'Bạn có thể đăng nhập lại bằng mật khẩu mới.':id?'Nhập mã được gửi đến email đã lưu của tài khoản và đặt mật khẩu mới.':'Nhập tên đăng nhập. Mã khôi phục sẽ được gửi đến email đã lưu trong tài khoản; bạn không cần nhập lại email.'}</p>
 {notice&&<p className="info-note" role="status">{notice}</p>}
 {!done&&<form key={id?'reset-'+id:'request'} autoComplete="off" aria-label={id?'Đặt lại mật khẩu':'Yêu cầu mã khôi phục'} onSubmit={async event=>{
  event.preventDefault();const data=new FormData(event.currentTarget);setBusy(true);setError('');setServiceUnavailable(false);
  try{if(!id){const result=await api.forgotPassword(String(data.get('name')).trim());setNewPassword('');setConfirmPassword('');setFocusedSecret(null);setId(result.maYeuCau);setNotice(result.thongBao);}
   else{if(newPassword!==confirmPassword)throw new Error('Mật khẩu xác nhận chưa khớp.');if(newPassword.length<8||!/[a-zA-Z]/.test(newPassword)||!/[0-9]/.test(newPassword))throw new Error('Mật khẩu cần ít nhất 8 ký tự, có chữ và số.');await api.resetPassword(id,String(data.get('code')).trim(),newPassword);clearSession();setNewPassword('');setConfirmPassword('');setDone(true);setNotice('Mật khẩu mới đã được lưu. Các phiên đăng nhập cũ đã bị thu hồi. Vui lòng đăng nhập lại.');}}
  catch(e){if(e instanceof ApiError&&e.status===503)setServiceUnavailable(true);else setError(e instanceof Error?e.message:'Chưa thực hiện được. Vui lòng thử lại.');}finally{setBusy(false)}
 }}><fieldset disabled={busy}>{!id?<>
 <label className="field"><span>Tên đăng nhập</span><input name="name" autoComplete="username" required maxLength={100}/></label>
 </>:<>
 <label className="field"><span>Mã 6 chữ số trong email</span><input key="recovery-code" name="code" type="text" inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}" minLength={6} maxLength={6} required/></label>
 <label className="field"><span>Mật khẩu mới từ 8 ký tự có chữ và số</span><input key={'new-'+id} name="recoveryNewPassword" type="password" autoComplete="new-password" value={newPassword} readOnly={focusedSecret!=='new'&&!newPassword} onFocus={()=>setFocusedSecret('new')} onBlur={()=>setFocusedSecret(null)} onChange={e=>setNewPassword(e.target.value)} minLength={8} maxLength={128} required/></label>
 <label className="field"><span>Nhập lại mật khẩu mới</span><input key={'confirm-'+id} name="recoveryConfirmPassword" type="password" autoComplete="new-password" value={confirmPassword} readOnly={focusedSecret!=='confirm'&&!confirmPassword} onFocus={()=>setFocusedSecret('confirm')} onBlur={()=>setFocusedSecret(null)} onChange={e=>setConfirmPassword(e.target.value)} minLength={8} maxLength={128} required/></label>
 </>}</fieldset><button className="btn btn-primary full" disabled={busy}>{busy?'Đang xử lý…':id?'Đặt mật khẩu mới':'Gửi mã qua email'}</button></form>}
 {error&&<p className="error-note" role="alert">{error}</p>}
 {serviceUnavailable&&<div className="info-note recovery-service-note" role="alert"><strong>Hiện chưa thể gửi mã khôi phục</strong><p>Dịch vụ email chưa sẵn sàng. Bạn có thể quay lại đăng nhập hoặc liên hệ trung tâm để được hỗ trợ.</p><p>Thông báo này không có nghĩa email bạn nhập sai.</p></div>}
 {id&&!done&&<button className="btn btn-soft full" type="button" disabled={busy} onClick={()=>{setId('');setError('');setNotice('Kiểm tra tên đăng nhập. Chờ ít nhất 60 giây trước khi yêu cầu mã mới; mã trước sẽ hết hiệu lực khi mã mới được tạo.')}}>Kiểm tra tên đăng nhập hoặc yêu cầu mã mới</button>}
 <button type="button" className="link-button account-switch" disabled={busy} onClick={onBack}>Quay lại đăng nhập</button></div>;
}

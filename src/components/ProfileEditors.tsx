import {PasswordEditor} from './PasswordEditor';
import {useEffect,useRef,useState} from 'react';
import type {StudentProfile} from '../types';
import {artCenterService as api,type Member} from '../services/artCenterService';
import './profileEditors.css';

const today=()=>new Date().toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'});
const errorMessage=(e:unknown)=>e instanceof Error?e.message:'Chưa lưu được thông tin. Vui lòng thử lại.';
export function StudentEditor({student,onSaved,onCancel}:{student:StudentProfile;onSaved:()=>Promise<void>;onCancel:()=>void}){
 const [busy,setBusy]=useState(false),[error,setError]=useState('');
 const heading=useRef<HTMLHeadingElement>(null);
 useEffect(()=>{heading.current?.focus();heading.current?.scrollIntoView({block:'nearest'})},[]);
 return <form className="profile-editor" aria-label="Sửa hồ sơ học viên" onSubmit={async e=>{
  e.preventDefault();const data=new FormData(e.currentTarget);setBusy(true);setError('');
  try{await api.updateStudent(student.id,{HoTen:String(data.get('name')).trim(),NgaySinh:data.get('birth'),LienHeKhanCapTen:String(data.get('emergency')).trim(),LienHeKhanCapSDT:String(data.get('phone')).trim(),MucTieuHoc:student.goal||null,RowVersion:student.rowVersion});await onSaved()}
  catch(e){setError(errorMessage(e))}finally{setBusy(false)}
 }}><h3 ref={heading} tabIndex={-1}>Sửa hồ sơ học viên</h3><p>Điều chỉnh thông tin của {student.name}. Lớp học và lịch sử học tập được giữ nguyên.</p>
 <fieldset disabled={busy}><div className="form-grid">
 <label className="field"><span>Họ tên học viên</span><input name="name" defaultValue={student.name} required maxLength={150}/></label>
 <label className="field"><span>Ngày sinh</span><input name="birth" type="date" defaultValue={student.birthDate.slice(0,10)} required min="1900-01-01" max={today()}/></label>
 <label className="field"><span>Người liên hệ khẩn cấp</span><input name="emergency" defaultValue={student.emergencyName||''} required maxLength={150}/></label>
 <label className="field"><span>Số điện thoại khẩn cấp</span><input name="phone" type="tel" defaultValue={student.emergencyPhone||''} required maxLength={25}/></label>
 </div></fieldset>{error&&<p className="error-note" role="alert">{error}</p>}<div className="profile-editor-actions"><button type="button" className="btn btn-soft" disabled={busy} onClick={onCancel}>Hủy chỉnh sửa</button><button className="btn btn-primary" disabled={busy}>{busy?'Đang lưu…':'Lưu thay đổi'}</button></div></form>;
}

export function AccountEditor({member,onSaved,onLogout,onPasswordChanged}:{member:Member;onSaved:(m:Member)=>void;onLogout:()=>Promise<void>;onPasswordChanged:()=>void}){
 const [account,setAccount]=useState<Member|null>(null),[busy,setBusy]=useState(false),[error,setError]=useState(''),[notice,setNotice]=useState('');
 useEffect(()=>{let alive=true;api.me().then(m=>{if(alive)setAccount(m)}).catch(e=>{if(alive)setError(errorMessage(e))});return()=>{alive=false}},[]);
 return <div className="modal-body"><span className="kicker">TÀI KHOẢN CỦA BẠN</span><h2>Thông tin cá nhân</h2><p>Tên đăng nhập: <b>{member.tenDangNhap}</b></p>{error&&<p className="error-note" role="alert">{error}</p>}{notice&&<p className="info-note" role="status">{notice}</p>}
 {!account?<p role="status">Đang tải thông tin…</p>:!account.thanhVienId?<p>Thông tin nhân sự được quản lý bởi trung tâm.</p>:<form key={account.rowVersion} onSubmit={async e=>{
  e.preventDefault();const data=new FormData(e.currentTarget);setBusy(true);setError('');setNotice('');
  try{await api.updateMember({HoTen:String(data.get('name')).trim(),DienThoai:String(data.get('phone')).trim(),Email:data.get('email')||null,NgaySinh:data.get('birth')||null,RowVersion:account.rowVersion});const fresh=await api.me();setAccount(fresh);sessionStorage.setItem('bui.profile.'+fresh.tenDangNhap,JSON.stringify(fresh));onSaved(fresh);setNotice('Đã lưu thông tin. Hồ sơ học viên được chỉnh sửa riêng trong khu vực Học tập.');}
  catch(e){setError(errorMessage(e))}finally{setBusy(false)}
 }}><fieldset disabled={busy}><div className="form-grid">
 <label className="field"><span>Họ tên</span><input name="name" defaultValue={account.hoTen||''} required maxLength={150}/></label>
 <label className="field"><span>Số điện thoại</span><input name="phone" type="tel" defaultValue={account.dienThoai||''} required maxLength={25}/></label>
 <label className="field"><span>Ngày sinh</span><input name="birth" type="date" defaultValue={account.ngaySinh?.slice(0,10)||''} min="1900-01-01" max={today()} required/></label>
 <label className="field"><span>Email (không bắt buộc)</span><input name="email" type="email" defaultValue={account.email||''} maxLength={254}/></label>
 </div></fieldset><button className="btn btn-primary full" disabled={busy}>{busy?'Đang lưu…':'Lưu thay đổi'}</button></form>}
 <PasswordEditor onChanged={onPasswordChanged}/>
 <button type="button" className="link-button profile-logout" disabled={busy} onClick={async()=>{setBusy(true);try{await onLogout()}catch(e){setError(errorMessage(e));setBusy(false)}}}>Đăng xuất</button></div>;
}

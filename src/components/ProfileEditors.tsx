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
 const [account,setAccount]=useState<Member|null>(null),[busy,setBusy]=useState(false),[error,setError]=useState(''),[notice,setNotice]=useState(''),[editing,setEditing]=useState(false);
 const [draft,setDraft]=useState({name:'',phone:'',birth:'',email:''});
 const dirty=!!account&&(draft.name.trim()!==(account.hoTen||'')||draft.phone.trim()!==(account.dienThoai||'')||draft.birth!==(account.ngaySinh?.slice(0,10)||'')||draft.email.trim()!==(account.email||''));
 const startEditing=()=>{if(!account)return;setDraft({name:account.hoTen||'',phone:account.dienThoai||'',birth:account.ngaySinh?.slice(0,10)||'',email:account.email||''});setEditing(true);setError('');setNotice('');};
 useEffect(()=>{let alive=true;api.me().then(m=>{if(alive)setAccount(m)}).catch(e=>{if(alive)setError(errorMessage(e))});return()=>{alive=false}},[]);
 return <div className="modal-body account-profile"><span className="kicker">TÀI KHOẢN CỦA BẠN</span><h2>{editing?'Chỉnh sửa thông tin':'Thông tin cá nhân'}</h2><p>Tên đăng nhập: <b>{member.tenDangNhap}</b></p>{error&&<p className="error-note" role="alert">{error}</p>}{notice&&<p className="info-note" role="status">{notice}</p>}
 {!account?<p role="status">Đang tải thông tin…</p>:!account.thanhVienId?<p>Thông tin nhân sự được quản lý bởi trung tâm.</p>:!editing?<>
  <dl className="account-profile-info"><div><dt>Họ tên</dt><dd>{account.hoTen||'Chưa bổ sung'}</dd></div><div><dt>Số điện thoại</dt><dd>{account.dienThoai||'Chưa bổ sung'}</dd></div><div><dt>Ngày sinh</dt><dd>{account.ngaySinh?new Date(account.ngaySinh.slice(0,10)+'T00:00:00+07:00').toLocaleDateString('vi-VN',{timeZone:'Asia/Ho_Chi_Minh'}):'Chưa bổ sung'}</dd></div><div><dt>Email</dt><dd>{account.email||'Chưa bổ sung email'}</dd></div></dl>
  <button type="button" className="btn btn-primary full" onClick={startEditing}>Chỉnh sửa thông tin</button>
 </>:<form className="account-profile-form" aria-label="Chỉnh sửa thông tin cá nhân" onSubmit={async e=>{
  e.preventDefault();if(!dirty||busy)return;setBusy(true);setError('');setNotice('');
  try{await api.updateMember({HoTen:draft.name.trim(),DienThoai:draft.phone.trim(),Email:draft.email.trim(),NgaySinh:draft.birth,RowVersion:account.rowVersion});const fresh=await api.me();setAccount(fresh);sessionStorage.setItem('bui.profile.'+fresh.tenDangNhap,JSON.stringify(fresh));onSaved(fresh);setEditing(false);setNotice('Đã lưu thông tin. Hồ sơ học viên được chỉnh sửa riêng trong khu vực Học tập.');}
  catch(e){setError(errorMessage(e))}finally{setBusy(false)}
 }}><fieldset disabled={busy}><div className="form-grid">
 <label className="field"><span>Họ tên</span><input name="name" value={draft.name} onChange={e=>setDraft({...draft,name:e.target.value})} required maxLength={150}/></label>
 <label className="field"><span>Số điện thoại</span><input name="phone" type="tel" value={draft.phone} onChange={e=>setDraft({...draft,phone:e.target.value})} required maxLength={25}/></label>
 <label className="field"><span>Ngày sinh</span><input name="birth" type="date" value={draft.birth} onChange={e=>setDraft({...draft,birth:e.target.value})} min="1900-01-01" max={today()} required/></label>
 <label className="field"><span>Email</span><input name="email" type="email" value={draft.email} onChange={e=>setDraft({...draft,email:e.target.value})} required maxLength={254}/></label>
 </div></fieldset><div className="account-profile-actions"><button type="button" className="btn btn-soft" disabled={busy} onClick={()=>{setEditing(false);setError('');setNotice('')}}>Hủy chỉnh sửa</button><button className="btn btn-primary" disabled={busy||!dirty}>{busy?'Đang lưu…':'Lưu thay đổi'}</button></div></form>}
 <PasswordEditor onChanged={onPasswordChanged}/>
 <button type="button" className="link-button profile-logout" disabled={busy} onClick={async()=>{setBusy(true);try{await onLogout()}catch(e){setError(errorMessage(e));setBusy(false)}}}>Đăng xuất</button></div>;
}

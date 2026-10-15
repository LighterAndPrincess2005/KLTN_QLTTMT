import {useState} from 'react';
import {Check, Trash2, UserRound} from 'lucide-react';
import type {StudentProfile} from '../types';
import {artCenterService as api} from '../services/artCenterService';

type Props={students:StudentProfile[];selectedId:string;busy:boolean;onSelect:(id:string)=>void;onBusy:(value:boolean)=>void;onError:(message:string)=>void;onDeleted:(id:string)=>Promise<void>};
const birthDate=(value:string)=>new Date(value+'T00:00:00+07:00').toLocaleDateString('vi-VN');
function years(value:string){const birth=new Date(value+'T00:00:00+07:00'),now=new Date();return now.getFullYear()-birth.getFullYear()-(now.getMonth()<birth.getMonth()||now.getMonth()===birth.getMonth()&&now.getDate()<birth.getDate()?1:0);}

export function StudentChoices({students,selectedId,busy,onSelect,onBusy,onError,onDeleted}:Props){
 const [confirmId,setConfirmId]=useState<string|null>(null);
 const remove=async(student:StudentProfile)=>{
  onBusy(true);onError('');
  try{await api.deleteStudent(student.id);setConfirmId(null);await onDeleted(student.id);}
  catch(error){onError(error instanceof Error?error.message:'Chưa xóa được hồ sơ. Vui lòng thử lại.');}
  finally{onBusy(false);}
 };
 return <div className="student-choices">{students.map(student=><div key={student.id} className={'student-choice'+(selectedId===student.id?' selected':'')}>
  <button type="button" className="student-select" disabled={busy} aria-pressed={selectedId===student.id} aria-label={`Chọn ${student.name}, sinh ngày ${birthDate(student.birthDate)}`} onClick={()=>onSelect(student.id)}>
   <UserRound aria-hidden="true"/><span className="student-summary"><b>{student.name}</b><small>{student.verified?student.level:'Chờ xác minh'}</small><span className="student-age">{years(student.birthDate)} tuổi · {birthDate(student.birthDate)}</span></span><Check className="student-selected-icon" aria-hidden="true"/>
  </button>
  <button type="button" className="student-delete" disabled={busy||!student.canDelete} aria-label={`Xóa hồ sơ ${student.name}, sinh ngày ${birthDate(student.birthDate)}`} title={student.canDelete?'Xóa hồ sơ chưa sử dụng':'Hồ sơ đã được trung tâm xử lý hoặc có lịch sử; liên hệ trung tâm để điều chỉnh.'} onClick={()=>setConfirmId(student.id)}><Trash2 size={16} aria-hidden="true"/> Xóa</button>
  {confirmId===student.id&&<div className="student-delete-confirm"><p>Xóa hồ sơ <b>{student.name}</b>, sinh ngày {birthDate(student.birthDate)} khỏi danh sách?</p><div><button type="button" className="cancel-delete" disabled={busy} onClick={()=>setConfirmId(null)}>Hủy</button><button type="button" className="confirm-delete" disabled={busy} onClick={()=>void remove(student)}>{busy?'Đang xóa…':'Xóa hồ sơ'}</button></div></div>}
 </div>)}</div>;
}

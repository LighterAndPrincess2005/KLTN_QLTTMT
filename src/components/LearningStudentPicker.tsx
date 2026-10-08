import {useEffect,useState} from 'react';
import {ArrowRight,GraduationCap,UserRound} from 'lucide-react';
import type {StudentProfile} from '../types';
import {artCenterService as api} from '../services/artCenterService';
import type {Enrollment,Member} from '../services/artCenterService';

export function LearningStudentPicker({member,onSelect,onRegister}:{member:Member;onSelect:(id:string)=>void;onRegister:()=>void}){
 const [students,setStudents]=useState<StudentProfile[]>([]),[rows,setRows]=useState<Enrollment[]>([]);
 const [loading,setLoading]=useState(true),[error,setError]=useState(''),[retry,setRetry]=useState(0);
 useEffect(()=>{
  let alive=true;setLoading(true);setError('');setStudents([]);setRows([]);
  Promise.all([api.getStudentProfiles(),api.enrollments()]).then(([profiles,enrollments])=>{
   if(alive){setStudents(profiles);setRows(enrollments.filter(row=>row.trangThai==='DA_XAC_NHAN'));}
  }).catch(e=>{if(alive)setError(e instanceof Error?e.message:'Chưa tải được danh sách học viên.');})
   .finally(()=>{if(alive)setLoading(false)});
  return()=>{alive=false};
 },[member.id,retry]);
 const eligible=students.filter(student=>rows.some(row=>row.hocVienId===Number(student.id)));
 return <div className="modal-body learning-picker">
  <span className="kicker">GÓC HỌC TẬP</span><h2>Bạn muốn xem học tập của ai?</h2>
  <p>Chọn học viên đã có lớp được xác nhận để xem lịch, điểm danh, nhận xét và các đề nghị học tập riêng.</p>
  {loading?<p className="learning-empty" role="status">Đang tải danh sách học viên có lớp…</p>
   :error?<div className="error-note" role="alert"><p>{error}</p><button className="btn btn-soft" onClick={()=>setRetry(n=>n+1)}>Thử lại</button></div>
   :eligible.length?<div className="family-students" role="group" aria-label="Học viên có lớp đã xác nhận">{eligible.map(student=>{
    const classes=rows.filter(row=>row.hocVienId===Number(student.id));
    const birth=new Date(student.birthDate+'T00:00:00+07:00').toLocaleDateString('vi-VN');
    return <button type="button" className="family-student learning-picker-student" key={student.id} data-student-id={student.id} aria-label={`Xem học tập của ${student.name}, sinh ngày ${birth}`} onClick={()=>onSelect(student.id)}>
     <span className="family-avatar"><UserRound size={24}/></span>
     <span className="family-student-info"><b>{student.name}</b><small>Sinh ngày {birth}</small><span className="profile-verified">{classes.length} lớp đã xác nhận</span><small>{classes.map(row=>`${row.tenKhoa} · ${row.tenLop}`).join(' / ')}</small></span>
     <ArrowRight size={20} className="picker-arrow"/>
    </button>;
   })}</div>
   :<div className="learning-welcome"><GraduationCap size={32}/><h3>Chưa có học viên được xác nhận vào lớp.</h3><p>Hồ sơ đã xác minh chưa đồng nghĩa đã vào lớp. Danh sách học tập sẽ hiện sau khi học viên đăng ký và trung tâm xác nhận đủ học phí.</p><button className="btn btn-primary" onClick={onRegister}>Đăng ký lớp <ArrowRight size={16}/></button></div>}
 </div>;
}

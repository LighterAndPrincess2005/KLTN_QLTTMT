import {useEffect,useRef,useState} from 'react';
import type {FormEvent} from 'react';
import {safeMediaUrl} from '../mediaUrls';
import {publicMediaUrl,request} from '../services/artCenterService';

type TeacherPhoto={url:string;congKhai:boolean};
const photoPath='/api/tep/cong-khai/giao-vien/toi';
const message=(error:unknown)=>error instanceof Error?error.message:'Chưa lưu được ảnh. Vui lòng thử lại.';

export function TeacherProfilePhoto(){
 const [photo,setPhoto]=useState<TeacherPhoto|null>(null);
 const [file,setFile]=useState<File|null>(null);
 const [busy,setBusy]=useState(false);
 const [error,setError]=useState('');
 const [saved,setSaved]=useState(false);
 const fileInput=useRef<HTMLInputElement>(null);

 useEffect(()=>{
  let active=true;
  request<TeacherPhoto>(photoPath).then(value=>{if(active)setPhoto(value)}).catch(error=>{if(active)setError(message(error))});
  return()=>{active=false};
 },[]);

 async function save(event:FormEvent<HTMLFormElement>){
  event.preventDefault();
  if(!file||busy)return;
  setError('');
  setSaved(false);
  if(file.size>5*1024*1024){setError('Ảnh tối đa 5 MB.');return}
  setBusy(true);
  try{
   const body=new FormData();
   body.append('file',file);
   setPhoto(await request<TeacherPhoto>(photoPath,body));
   setFile(null);
   if(fileInput.current)fileInput.current.value='';
   setSaved(true);
  }catch(error){setError(message(error))}
  finally{setBusy(false)}
 }

 return <details className="teacher-public-photo">
  <summary>Hồ sơ công khai</summary>
  {!photo&&!error&&<p role="status">Đang tải ảnh đại diện…</p>}
  {photo&&<form onSubmit={save}>
   {safeMediaUrl(photo.url)&&<img src={publicMediaUrl(photo.url)} alt="Ảnh đại diện hiện tại" width={72} height={72}/>}
   <p>{photo.congKhai?'Ảnh sẽ xuất hiện ngay ở trang Giới thiệu sau khi lưu.':'Hồ sơ đang ẩn trên trang Giới thiệu. Bạn vẫn có thể cập nhật ảnh.'}</p>
   <label className="field"><span>Chọn ảnh đại diện</span><input ref={fileInput} type="file" accept="image/png,image/jpeg,image/webp" disabled={busy} onChange={event=>{setFile(event.target.files?.[0]??null);setSaved(false);setError('')}}/></label>
   <button className="btn btn-primary" disabled={!file||busy}>{busy?'Đang lưu…':'Lưu ảnh'}</button>
   {saved&&<p role="status">Đã cập nhật ảnh đại diện.</p>}
  </form>}
  {error&&<p role="alert" className="error-note">{error}</p>}
 </details>;
}

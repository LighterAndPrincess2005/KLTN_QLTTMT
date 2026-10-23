import {useEffect,useState} from 'react';
import {artCenterService as api} from '../services/artCenterService';
import type {Enrollment} from '../services/artCenterService';
import type {ArtClass} from '../types';

export function ClassStudyPeriod({enrollment,classes}:{enrollment:Enrollment;classes:ArtClass[]}){
 const cls=classes.find(c=>c.id===String(enrollment.lopHocId));
 const [scheduled,setScheduled]=useState<string[]>([]);
 const start=enrollment.ngayBatDau||cls?.startDateISO,end=enrollment.ngayKetThuc||cls?.endDateISO;
 useEffect(()=>{let alive=true;setScheduled([]);if(!start||!end)api.progress(enrollment.hoSoTheoHocId).then(p=>{if(alive)setScheduled(p.diemDanh.filter(r=>r.loaiThamGia!=='HOC_BU'&&r.trangThai!=='HUY_BO'&&r.trangThaiBuoi!=='DA_HUY').map(r=>r.ngayHoc.slice(0,10)).sort())}).catch(()=>{});return()=>{alive=false}},[start,end,enrollment.hoSoTheoHocId]);
 const month=(value:string)=>value.slice(5,7)+'/'+value.slice(0,4);
 if(start&&end)return <span className="study-class-period">Dự kiến: {month(start)} – {month(end)}</span>;
 if(scheduled.length)return <span className="study-class-period">Lịch đã xếp: {month(scheduled[0])} – {month(scheduled.at(-1)!)}</span>;
 return <span className="study-class-period">{start?'Bắt đầu: '+month(start)+' · Chưa có tháng kết thúc':'Chưa công bố thời gian học'}</span>;
}

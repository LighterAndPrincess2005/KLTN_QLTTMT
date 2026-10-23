import type {ArtClass} from './types';
export function isPublicClass(cls:ArtClass,now=new Date()):boolean{
 if(!['Đang nhận đăng ký','Đủ chỗ'].includes(cls.status))return false;
 const today=now.toLocaleDateString('en-CA',{timeZone:'Asia/Ho_Chi_Minh'});
 if(cls.startDateISO&&cls.startDateISO<today)return false;
 if(cls.deadline){const deadline=new Date(/(?:Z|[+-]\d\d:\d\d)$/.test(cls.deadline)?cls.deadline:cls.deadline+'+07:00');if(!Number.isFinite(deadline.getTime())||deadline<=now)return false;}
 return true;
}
export const canChoosePublicClass=(cls:ArtClass)=>isPublicClass(cls)&&cls.status==='Đang nhận đăng ký'&&cls.enrolled<cls.capacity;

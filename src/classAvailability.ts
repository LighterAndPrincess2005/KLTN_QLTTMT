import type {ArtClass} from './types';

export const remainingSeats=(cls:ArtClass)=>Math.max(0,cls.capacity-cls.enrolled);
export const isAlmostFull=(cls:ArtClass)=>cls.status==='Đang nhận đăng ký'&&remainingSeats(cls)>=1&&remainingSeats(cls)<=3;
export function classAvailability(cls:ArtClass){
 if(cls.status==='Đủ chỗ'||remainingSeats(cls)===0)return {label:'Đã đủ chỗ',tone:'full'};
 if(isAlmostFull(cls))return {label:'Sắp hết chỗ',tone:'limited'};
 return {label:cls.status,tone:'open'};
}

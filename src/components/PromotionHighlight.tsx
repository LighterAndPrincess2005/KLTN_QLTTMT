import {Tag} from 'lucide-react';
import type {ArtClass} from '../types';

export function PromotionHighlight({promotion,variant='inline'}:{promotion:NonNullable<ArtClass['promotion']>;variant?:'sticker'|'inline'}) {
 const percent=promotion.percent.toLocaleString('vi-VN',{maximumFractionDigits:2});
 return <div className={`tuition-offer tuition-offer--${variant}`} title={`${promotion.label}. Tự động áp dụng, không cần mã.`} aria-label={`Ưu đãi học phí ${percent} phần trăm. ${promotion.label}. Tự động áp dụng, không cần mã`}>
   {variant==='sticker'?<><span className="tuition-offer-caption">ƯU ĐÃI</span><strong className="tuition-offer-value">−{percent}<span>%</span></strong><span className="tuition-offer-foot">HỌC PHÍ</span></>:<><Tag size={13} aria-hidden="true"/><strong className="tuition-offer-value">−{percent}<span>%</span></strong><span>học phí</span></>}
 </div>;
}

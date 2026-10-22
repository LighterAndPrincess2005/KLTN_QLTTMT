import {Tag} from 'lucide-react';
import type {ArtClass} from '../types';

export function PromotionHighlight({promotion}:{promotion:NonNullable<ArtClass['promotion']>}) {
 const percent=promotion.percent.toLocaleString('vi-VN',{maximumFractionDigits:2});
 return <div className="promotion-highlight" aria-label={`Ưu đãi học phí ${percent} phần trăm. ${promotion.label}. Tự động áp dụng`}>
   <div className="promotion-value"><Tag size={18} aria-hidden="true"/><strong>−{percent}<span>%</span></strong></div>
   <div className="promotion-description"><span className="promotion-eyebrow">ƯU ĐÃI HỌC PHÍ</span><b>Thêm sắc màu, nhẹ học phí.</b><span className="promotion-code">Tự động áp dụng · không cần mã</span></div>
 </div>;
}

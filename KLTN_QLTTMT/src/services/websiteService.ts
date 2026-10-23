import {useEffect,useState} from 'react';
import {request} from './artCenterService';
import {demoMode} from './demo';
import {artArticles as demoArticles,type ArtArticle} from '../data/artArticles';
export type PublicTeacher={id:number;name:string;location:string;image:string;description:string;specialty:string;works:string};
export type AdviceReceipt={maTraCuu:string;khoaTraCuu:string;trangThai:string;guiLuc:string};
export type AdviceDetail={maTraCuu:string;chuDe:string;trangThai:string;guiLuc:string;docLuc:string|null;rowVersion:string;tinNhan:{id:number;noiDung:string;tuNhanVien:boolean;guiLuc:string}[]};
export type AdviceRow={id:number;maTraCuu:string;hoTen:string;email:string;dienThoai:string|null;chuDe:string;trangThai:string;guiLuc:string;rowVersion:string};
export type Quote={giamKhoaHoc?:number;giamMaCaNhan?:number;hocPhiGoc:number;tienGiam:number;tongPhaiTra:number;maGiamHocPhi:string|null;hanGiuChoDuKien:string;quyDinh:{hanGiuChoGio:number;baoNghiTruocGio:number;soLuotHocBu:number;hanHocBuNgay:number;soLanChuyen:number};thongBao:string};
export const websiteApi={
 articles:()=>demoMode?Promise.resolve(demoArticles):request<ArtArticle[]>('/api/noi-dung/bai-viet'),
 teachers:()=>request<PublicTeacher[]>('/api/noi-dung/giao-vien'),
 sendAdvice:(body:unknown)=>request<AdviceReceipt>('/api/tu-van',body),
 lookup:(code:string,secret:string)=>request<AdviceDetail>('/api/tu-van/tra-cuu',{MaTraCuu:code,KhoaTraCuu:secret}),
 followup:(code:string,secret:string,text:string)=>request('/api/tu-van/bo-sung',{MaTraCuu:code,KhoaTraCuu:secret,NoiDung:text}),
 mine:()=>request<{id:number;maTraCuu:string;chuDe:string;trangThai:string}[]>('/api/tu-van/toi'),
 ownAdvice:(id:number)=>request<AdviceDetail>('/api/tu-van/toi/'+id),
 ownFollowup:(id:number,text:string,version:string)=>request(`/api/tu-van/toi/${id}/bo-sung`,{NoiDung:text,RowVersion:version}),
 listAdvice:(status:string,page:number)=>request<{tongSo:number;duLieu:AdviceRow[]}>(`/api/tu-van?trang=${page}&kichThuoc=20&trangThai=${encodeURIComponent(status)}`),
 readAdvice:(id:number)=>request<AdviceDetail>('/api/tu-van/'+id),
 replyAdvice:(id:number,text:string,version:string)=>request(`/api/tu-van/${id}/tra-loi`,{NoiDung:text,RowVersion:version}),
 quote:(studentId:string,classId:string,code:string)=>request<Quote>('/api/dang-ky/bao-gia',{HocVienId:Number(studentId),LopHocId:Number(classId),MaGiamHocPhi:code.trim().toUpperCase()||null}),
 debt:(id:number)=>request<{dangKyId:number;trangThai:string;khoanThu:{id:number;loaiKhoan:string;thanhTienChot:number;daThanhToan:number;conThieu:number}[]}>(`/api/dang-ky/${id}/cong-no`)
};
export function useArticles(){const[items,setItems]=useState<ArtArticle[]>([]),[loading,setLoading]=useState(true),[error,setError]=useState('');useEffect(()=>{let alive=true;websiteApi.articles().then(rows=>{if(alive)setItems(rows)}).catch(e=>{if(alive)setError(e instanceof Error?e.message:'Chưa tải được bài viết.')}).finally(()=>{if(alive)setLoading(false)});return()=>{alive=false}},[]);return{items,loading,error}}

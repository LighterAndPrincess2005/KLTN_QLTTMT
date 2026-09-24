import type { ArtClass, Course } from '../types';

export const courses: Course[] = [
  { id:'junior', title:'Junior Sắc Màu', age:'3–5 tuổi', duration:'90 phút/buổi', sessions:12, price:1800000, materialFee:250000, category:'Nền tảng', color:'#f79b84', image:'https://images.unsplash.com/photo-1596464716127-f2a82984de30?auto=format&fit=crop&w=900&q=85', skills:['Cảm nhận màu sắc','Vận động tinh','Tự tin biểu đạt'] },
  { id:'foundation', title:'Foundation Studio', age:'6–8 tuổi', duration:'120 phút/buổi', sessions:16, price:2800000, materialFee:350000, category:'Nền tảng', color:'#e9b949', image:'https://images.unsplash.com/photo-1541961017774-22349e4a1262?auto=format&fit=crop&w=900&q=85', skills:['Hình khối cơ bản','Phối màu','Kể chuyện bằng tranh'] },
  { id:'basic', title:'Basic & Pre-Basic', age:'9–12 tuổi', duration:'120 phút/buổi', sessions:16, price:2800000, materialFee:400000, category:'Hội họa', color:'#54a88d', image:'https://images.unsplash.com/photo-1513364776144-60967b0f800f?auto=format&fit=crop&w=900&q=85', skills:['Bố cục','Màu nước','Quan sát & ký họa'] },
  { id:'advanced', title:'Young Artist Lab', age:'13–17 tuổi', duration:'120 phút/buổi', sessions:16, price:3200000, materialFee:450000, category:'Hội họa', color:'#7367ad', image:'https://images.unsplash.com/photo-1579783902614-a3fb3927b6a5?auto=format&fit=crop&w=900&q=85', skills:['Ngôn ngữ cá nhân','Portfolio','Đa chất liệu'] },
  { id:'acrylic', title:'Acrylic Playground', age:'9–17 tuổi', duration:'120 phút/buổi', sessions:8, price:1800000, materialFee:450000, category:'Hội họa', color:'#ed765e', image:'https://images.unsplash.com/photo-1577083552431-6e5fd01988a5?auto=format&fit=crop&w=900&q=85', skills:['Pha màu','Kỹ thuật cọ','Tranh canvas'] },
  { id:'clay', title:'Clay & Craft', age:'5–12 tuổi', duration:'90 phút/buổi', sessions:4, price:1200000, materialFee:300000, category:'Thủ công', color:'#4f95bb', image:'https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?auto=format&fit=crop&w=900&q=85', skills:['Tạo hình 3D','Khéo léo','Tư duy không gian'] },
];

export const classes: ArtClass[] = [
  { id:'L-JR-2601', courseId:'junior', schedule:'Thứ 7 · 08:00–09:30', startDate:'05/10/2026', teacher:'Cô Khánh Linh', room:'Phòng Mây', capacity:8, enrolled:6, mode:'Trực tiếp' },
  { id:'L-FD-2601', courseId:'foundation', schedule:'Thứ 7 · 09:00–11:00', startDate:'05/10/2026', teacher:'Cô Nguyễn An', room:'Phòng Nắng', capacity:12, enrolled:8, mode:'Trực tiếp' },
  { id:'L-FD-2602', courseId:'foundation', schedule:'Chủ nhật · 14:00–16:00', startDate:'06/10/2026', teacher:'Cô Trần Mai', room:'Phòng Mây', capacity:12, enrolled:4, mode:'Trực tiếp' },
  { id:'L-BS-2601', courseId:'basic', schedule:'Thứ 7 · 14:00–16:00', startDate:'05/10/2026', teacher:'Thầy Lê Minh', room:'Phòng Nắng', capacity:12, enrolled:8, mode:'Trực tiếp' },
  { id:'L-AD-2601', courseId:'advanced', schedule:'Thứ 4 · 18:30–20:30', startDate:'09/10/2026', teacher:'Cô Gia Hân', room:'Studio 2', capacity:8, enrolled:3, mode:'Trực tuyến' },
  { id:'L-AC-2601', courseId:'acrylic', schedule:'Chủ nhật · 09:00–11:00', startDate:'13/10/2026', teacher:'Cô Phạm Hoa', room:'Studio 1', capacity:10, enrolled:4, mode:'Trực tiếp' },
  { id:'L-CL-2601', courseId:'clay', schedule:'Chủ nhật · 15:30–17:00', startDate:'13/10/2026', teacher:'Thầy Võ Anh', room:'Phòng Đất', capacity:10, enrolled:5, mode:'Trực tiếp' },
];

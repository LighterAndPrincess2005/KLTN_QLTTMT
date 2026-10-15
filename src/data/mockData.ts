import type { ArtClass, Course, LearningRecord, StudentProfile } from '../types';

export const courses:Course[]=[
{id:'junior',title:'Junior Sắc Màu',ageMin:3,ageMax:5,level:'Nhập môn',sessions:12,minutesPerSession:90,maxStudents:8,tuitionFee:1800000,materialFee:250000,category:'Nền tảng',color:'#f07965',image:'https://images.unsplash.com/photo-1596464716127-f2a82984de30?auto=format&fit=crop&w=900&q=85',skills:['Cảm nhận màu sắc','Vận động tinh','Tự tin biểu đạt'],prerequisites:[]},
{id:'foundation',title:'Foundation Studio',ageMin:6,ageMax:8,level:'Nền tảng',sessions:16,minutesPerSession:120,maxStudents:10,tuitionFee:2800000,materialFee:350000,category:'Nền tảng',color:'#d79a24',image:'https://images.unsplash.com/photo-1541961017774-22349e4a1262?auto=format&fit=crop&w=900&q=85',skills:['Hình khối cơ bản','Phối màu','Kể chuyện bằng tranh'],prerequisites:[]},
{id:'pre-basic',title:'Pre-Basic',ageMin:8,ageMax:11,level:'Sơ cấp',sessions:16,minutesPerSession:120,maxStudents:10,tuitionFee:2950000,materialFee:400000,category:'Hội họa',color:'#4b9d83',image:'https://images.unsplash.com/photo-1513364776144-60967b0f800f?auto=format&fit=crop&w=900&q=85',skills:['Bố cục','Màu nước','Quan sát'],prerequisites:['Foundation Studio hoặc đánh giá đầu vào đạt']},
{id:'basic',title:'Basic',ageMin:10,ageMax:14,level:'Cơ bản',sessions:16,minutesPerSession:120,maxStudents:10,tuitionFee:3200000,materialFee:450000,category:'Hội họa',color:'#655da6',image:'https://images.unsplash.com/photo-1579783902614-a3fb3927b6a5?auto=format&fit=crop&w=900&q=85',skills:['Bố cục nâng cao','Vẽ người','Đa chất liệu'],prerequisites:['Hoàn thành Pre-Basic hoặc đánh giá đầu vào đạt']},
{id:'acrylic',title:'Acrylic Playground',ageMin:9,ageMax:17,level:'Chuyên đề',sessions:8,minutesPerSession:120,maxStudents:10,tuitionFee:1800000,materialFee:450000,category:'Hội họa',color:'#ed765e',image:'https://images.unsplash.com/photo-1577083552431-6e5fd01988a5?auto=format&fit=crop&w=900&q=85',skills:['Pha màu','Kỹ thuật cọ','Tranh canvas'],prerequisites:[]},
{id:'clay',title:'Clay & Craft',ageMin:5,ageMax:12,level:'Chuyên đề',sessions:4,minutesPerSession:90,maxStudents:10,tuitionFee:1200000,materialFee:300000,category:'Thủ công',color:'#4f95bb',image:'https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?auto=format&fit=crop&w=900&q=85',skills:['Tạo hình 3D','Khéo léo','Tư duy không gian'],prerequisites:[]}
];

export const classes:ArtClass[]=[
{id:'L-JR-2601',courseId:'junior',schedule:'Thứ 7 · 08:00–09:30',startDate:'05/10/2026',registrationDeadline:'03/10/2026 08:00',teacher:'Cô Khánh Linh',room:'P01',capacity:8,enrolled:6,mode:'Trực tiếp',status:'Đang nhận đăng ký'},
{id:'L-FD-2601',courseId:'foundation',schedule:'Thứ 7 · 09:00–11:00',startDate:'05/10/2026',registrationDeadline:'03/10/2026 09:00',teacher:'Cô Nguyễn An',room:'P02',capacity:10,enrolled:10,mode:'Trực tiếp',status:'Đủ chỗ'},
{id:'L-FD-2602',courseId:'foundation',schedule:'Chủ nhật · 14:00–16:00',startDate:'12/10/2026',registrationDeadline:'10/10/2026 14:00',teacher:'Cô Trần Mai',room:'P01',capacity:10,enrolled:4,mode:'Trực tiếp',status:'Chưa mở đăng ký'},
{id:'L-PB-2601',courseId:'pre-basic',schedule:'Thứ 7 · 14:00–16:00',startDate:'05/10/2026',registrationDeadline:'03/10/2026 14:00',teacher:'Thầy Lê Minh',room:'P02',capacity:10,enrolled:8,mode:'Trực tiếp',status:'Đã khóa đăng ký'},
{id:'L-BS-2601',courseId:'basic',schedule:'Thứ 4 · 18:30–20:30',startDate:'09/09/2026',registrationDeadline:'07/09/2026 18:30',teacher:'Cô Gia Hân',room:'P02',capacity:10,enrolled:7,mode:'Trực tiếp',status:'Đang học'},
{id:'L-AC-2601',courseId:'acrylic',schedule:'Chủ nhật · 09:00–11:00',startDate:'13/10/2026',registrationDeadline:'11/10/2026 09:00',teacher:'Cô Phạm Hoa',room:'P01',capacity:10,enrolled:4,mode:'Trực tiếp',status:'Đang nhận đăng ký'},
{id:'L-CL-2501',courseId:'clay',schedule:'Chủ nhật · 15:30–17:00',startDate:'02/08/2026',registrationDeadline:'31/07/2026 15:30',teacher:'Thầy Võ Anh',room:'P01',capacity:10,enrolled:8,mode:'Trực tiếp',status:'Đã kết thúc'}
];

export const studentProfiles:StudentProfile[]=[
{id:'HV-001',name:'Nguyễn Bảo An',birthDate:'2017-06-18',level:'Nền tảng',completedCourseIds:['foundation'],existingSchedules:['Chủ nhật · 09:00–11:00']},
{id:'HV-002',name:'Nguyễn Minh Khang',birthDate:'2021-03-02',level:'Nhập môn',completedCourseIds:[],existingSchedules:[]}
];

export const learningRecord:LearningRecord={classId:'L-BS-2601',courseTitle:'Basic',schedule:'Thứ 4 · 18:30–20:30 · P02',attendance:{attended:9,absent:1,total:16},makeUpRequests:[{id:'HB-018',session:'Buổi 7 · Dựng hình tĩnh vật',status:'Đã xếp lịch'}],teacherComment:'Bảo An quan sát tốt, phối màu tự tin. Cần dành thêm thời gian cho tỷ lệ hình người.',products:['Tĩnh vật mùa thu','Thành phố trong mơ','Chân dung bạn em'],progress:62,completionStatus:'Đang học',certificateEligible:false};

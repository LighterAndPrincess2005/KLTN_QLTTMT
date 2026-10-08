# Ảnh minh họa LHL Art

## Bộ ảnh đang hiển thị — PPA (08/10/2026)

Theo yêu cầu của người dùng, web hiện dùng bốn ảnh từ phòng trưng bày PPA thay cho bộ ảnh tự tạo. Giao diện giữ nhãn ảnh minh họa, không tự tạo tên học viên và không tuyên bố là tác phẩm học viên LHL Art. Không hiển thị liên kết nguồn trên giao diện theo yêu cầu; lưu nguồn nội bộ trong `ppa-gallery-sources.json`. Giữ nguyên chữ ký/watermark của ảnh gốc. Chưa xác nhận quyền tái sử dụng ảnh từ PPA.

Các tệp dùng trên web: `public/gallery/ppa-watercolor-study.webp`, `public/gallery/ppa-still-life.webp`, `public/gallery/ppa-seascape.webp`, `public/gallery/ppa-flower-study.webp`.

Nguồn: https://ppa.vn/phong-trung-bay.html và https://ppa.vn/mau-nuoc-scabv17.html.

## Bộ ảnh tự tạo trước đó (được giữ để thay thế)

Bốn tranh gốc được tạo riêng bằng công cụ imagegen tích hợp cho khu thành phẩm và ảnh mở đầu. Đây là ảnh minh họa, không phải tác phẩm học viên LHL Art được xác thực. Bộ ảnh tự tạo trước đó không lấy ảnh từ trung tâm khác.

Tài sản dùng trên web nằm trong `public/gallery/`: `garden.webp`, `ocean.webp`, `city.webp`, `still-life.webp`. Các bản PNG gốc được giữ trong thư mục ảnh sinh của Codex. WebP chỉ thay đổi định dạng và kích thước phục vụ web, không thay đổi nội dung tranh. Giữ logo do người dùng cung cấp tại `public/logo-lhl-art.jpg`.

Khi có tác phẩm học viên thật, thay ảnh trong danh sách của `src/components/PublicSections.tsx`, cập nhật mô tả và bỏ ghi chú ảnh minh họa khi phù hợp.

## Prompt dùng tạo ảnh

### garden

Use case: illustration-story. Asset type: original sample artwork for a Vietnamese art center web gallery. Primary request: a single finished wax crayon outlines and watercolor wash, believable beginner school art, visible grain of paper artwork depicting a lush imaginary flower garden, orange and yellow blooms, a turquoise bird, small butterflies and abundant emerald foliage, joyful imperfect childlike composition. Composition: one flat rectangular painting filling the entire image, front-on reproduction with slight off-white paper border on all four edges. Attractive colors and believable handmade texture, simple confident strokes rather than hyperpolished digital illustration. NO people, NO hands, NO frame, NO room, NO collage, NO text, NO signature, NO watermark, NO branding. Create an ORIGINAL artwork, do not imitate or reproduce any specific existing artwork.

### ocean

Use case: illustration-story. Asset type: original sample artwork for a Vietnamese art center web gallery. Primary request: a single finished watercolor and colored pencil outlines, vivid blue and coral orange, charming childlike illustration with visible paper artwork depicting a smiling sea turtle, colorful tropical fish and coral in an underwater blue world, playful naive hand-drawn shapes. Composition: one flat rectangular painting filling the entire image, front-on reproduction with slight off-white paper border on all four edges. Attractive colors and believable handmade texture, simple confident strokes rather than hyperpolished digital illustration. NO people, NO hands, NO frame, NO room, NO collage, NO text, NO signature, NO watermark, NO branding. Create an ORIGINAL artwork, do not imitate or reproduce any specific existing artwork.

### city

Use case: illustration-story. Asset type: original sample artwork for a Vietnamese art center web gallery. Primary request: a single finished student-level opaque gouache with visible imperfect brushstrokes and hand-drawn lines, bright warm colors artwork depicting a whimsical Vietnamese neighborhood street with crooked ochre and coral houses, mint windows, terracotta rooftops, potted plants and a bicycle, sunny sky. Composition: one flat rectangular painting filling the entire image, front-on reproduction with slight off-white paper border on all four edges. Attractive colors and believable handmade texture, simple confident strokes rather than hyperpolished digital illustration. NO people, NO hands, NO frame, NO room, NO collage, NO text, NO signature, NO watermark, NO branding. Create an ORIGINAL artwork, do not imitate or reproduce any specific existing artwork.

### still-life

Use case: illustration-story. Asset type: original sample artwork for a Vietnamese art center web gallery. Primary request: a single finished beginner acrylic on textured paper, tactile brush strokes, accessible charming proportions, deep blue and warm orange artwork depicting a simple still life of a cobalt blue vase with orange and cream flowers, three oranges and a softly draped checked cloth on a wooden table. Composition: one flat rectangular painting filling the entire image, front-on reproduction with slight off-white paper border on all four edges. Attractive colors and believable handmade texture, simple confident strokes rather than hyperpolished digital illustration. NO people, NO hands, NO frame, NO room, NO collage, NO text, NO signature, NO watermark, NO branding. Create an ORIGINAL artwork, do not imitate or reproduce any specific existing artwork.

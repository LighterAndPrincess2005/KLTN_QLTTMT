// React escapes labels; URL validation separately rejects executable schemes and path traversal.
export const safeMediaUrl=(url:string)=>/^https?:\/\//i.test(url)||/^\/(?:bui|uploads|api\/tep\/cong-khai)\//.test(url)&&!url.includes('..')&&!url.includes('\\');

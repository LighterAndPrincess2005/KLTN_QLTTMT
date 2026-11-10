export function pageHref(path:string){
 const [address,hash]=path.split('#');
 const [pathname,search]=address.split('?');
 const params=new URLSearchParams(search);
 return pathname+(params.size?'?'+params.toString():'')+(hash?'#'+hash:'');
}
export const courseHref=(id:string)=>pageHref('/khoa-hoc/'+encodeURIComponent(id));
export const courseClassesHref=(id:string)=>pageHref('/lop-hoc?course='+encodeURIComponent(id)+'#classes');

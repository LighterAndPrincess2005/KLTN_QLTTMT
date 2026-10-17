// Keep preview mode while navigating between real, reloadable page URLs.
export function pageHref(path:string){
 const [address,hash]=path.split('#');
 const [pathname,search]=address.split('?');
 const params=new URLSearchParams(search);
 if(new URLSearchParams(window.location.search).get('demo')==='1')params.set('demo','1');
 return pathname+(params.size?'?'+params.toString():'')+(hash?'#'+hash:'');
}
export const courseHref=(id:string)=>pageHref('/khoa-hoc/'+encodeURIComponent(id));
export const courseClassesHref=(id:string)=>pageHref('/lop-hoc?course='+encodeURIComponent(id)+'#classes');

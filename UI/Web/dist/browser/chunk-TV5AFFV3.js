import{a as We}from"./chunk-KFQ6GX7Y.js";import{a as Qe}from"./chunk-5TFMVASI.js";import{a as $e}from"./chunk-AKPBGRK2.js";import{a as ze}from"./chunk-DDQDPGQ7.js";import{a as Q}from"./chunk-P4LDGXKI.js";import{a as F}from"./chunk-NDJ6S27G.js";import{a as Ve}from"./chunk-QROG6LNA.js";import{a as Ce,f as Se}from"./chunk-MCMEI552.js";import{J as He,b as Te,c as Re,g as Ie,h as De,n as Me,p as Pe,q as Ee,v as Ae,w as Fe,x as Ne,y as Oe,z as Be}from"./chunk-T7Y5TR4X.js";import{i as _e,l as ye,m as xe,o as ke}from"./chunk-7VNJ7Y5U.js";import{h as we}from"./chunk-KJX5IEQE.js";import{Aa as x,Cb as N,Dc as y,Eb as R,Fc as d,Gc as me,Hb as L,Hc as ge,Ic as H,Jb as _,Jc as he,Ka as ae,Kc as E,Kd as $,Lc as A,Pc as S,Rc as K,Tc as V,Uc as c,Ud as Y,Vc as m,Wc as f,Zb as C,bc as O,cd as q,db as ce,dc as B,dd as U,ed as ue,gc as j,ha as ne,hc as G,ic as l,id as z,jb as r,jc as i,kc as a,lc as s,ma as u,nd as ve,od as fe,pc as se,qb as le,qc as be,rc as I,sa as k,sd as D,ta as w,uc as pe,vc as M,wc as P,ya as ie,yb as de}from"./chunk-66GPU3XZ.js";import{h as re}from"./chunk-J2SLNJRR.js";var Le=`
:root .brtheme-black {
  /* General */
  --color-scheme: dark;
  --bs-body-color: black;
  --hr-color: rgba(239, 239, 239, 0.125);
  --accent-bg-color: rgba(1, 4, 9, 0.5);
  --accent-text-color: lightgrey;
  --body-text-color: #efefef;
  --btn-icon-filter: invert(1) grayscale(100%) brightness(200%);

  /* Drawer */
  --drawer-bg-color: #292929;
  --drawer-text-color: white;
  --drawer-pagination-horizontal-rule: inset 0 -1px 0 rgb(255 255 255 / 20%);
  --drawer-pagination-border: 1px solid rgb(0 0 0 / 13%);
  

  /* Accordion */
  --accordion-surface-bg-color: black;
  --accordion-header-text-color: var(--body-text-color);
  --accordion-header-bg-color: transparent;
  --accordion-body-bg-color: var(--accordion-surface-bg-color);
  --accordion-body-border-color: rgba(239, 239, 239, 0.125);
  --accordion-body-text-color: var(--body-text-color);
  --accordion-header-collapsed-text-color: var(--body-text-color);
  --accordion-header-collapsed-bg-color: transparent;
  --accordion-button-focus-border-color: unset;
  --accordion-button-focus-box-shadow: unset;
  --accordion-active-body-bg-color: var(--accordion-surface-bg-color);
  --accordion-body-box-shadow: none;
  --accordion-hairline-color: rgba(255, 255, 255, 0.08);
  --accordion-subtitle-text-color: #8b95a5;
  --accordion-meta-text-color: rgba(255, 255, 255, 0.45);
  --accordion-chevron-color: #8b95a5;
  --accordion-chevron-hover-color: var(--body-text-color);
  --accordion-border-hover-color: rgba(255, 255, 255, 0.2);
  --accordion-radius: var(--card-border-radius);
  --accordion-gap: 0.75rem;

  /* Buttons */
    --btn-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-primary-text-color: white;
    --btn-primary-bg-color: var(--primary-color);
    --btn-primary-border-color: var(--primary-color);
    --btn-primary-hover-text-color: white;
    --btn-primary-hover-bg-color: var(--primary-color-darker-shade);
    --btn-primary-hover-border-color: var(--primary-color-darker-shade);
    --btn-alt-bg-color: #424c72;
    --btn-alt-border-color: #444f75;
    --btn-alt-hover-bg-color: #3b4466;
    --btn-alt-focus-bg-color: #343c59;
    --btn-alt-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-fa-icon-color: white;
    --btn-disabled-bg-color: #343a40;
    --btn-disabled-text-color: white;
    --btn-disabled-border-color: #6c757d;

    /* Inputs */
    --input-bg-color: #343a40;
    --input-bg-readonly-color: #434648;
    --input-focused-border-color: #ccc;
    --input-text-color: #fff;
    --input-placeholder-color: #aeaeae;
    --input-border-color: #ccc;
    --input-focus-boxshadow-color: rgb(255 255 255 / 50%);

    /* Nav (Tabs) */
    --nav-tab-border-color: rgba(44, 118, 88, 0.7);
    --nav-tab-text-color: var(--body-text-color);
    --nav-tab-bg-color: var(--primary-color);
    --nav-tab-hover-border-color: var(--primary-color);
    --nav-tab-active-text-color: white;
    --nav-tab-border-hover-color: transparent;
    --nav-tab-hover-text-color: var(--body-text-color);
    --nav-tab-hover-bg-color: transparent;
    --nav-tab-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-border-bottom: rgba(44, 118, 88, 0.7);
    --nav-tab-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-bottom: var(--bs-body-bg);
    --nav-tab-hover-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-active-hover-bg-color: var(--primary-color);
    --nav-link-bg-color: var(--primary-color);
    --nav-link-active-text-color: white;
    --nav-link-text-color: white;



  /* Reading Bar */
  --br-actionbar-button-text-color: white;
  --br-actionbar-button-hover-border-color: #6c757d;
  --br-actionbar-bg-color: black;
}



.book-content *:not(input), .book-content *:not(select), .book-content *:not(code), .book-content *:not(:link), .book-content *:not(.ngx-toastr) {
  color: #dcdcdc !important;
}

.book-content code {
  color: #e83e8c !important;
}

.book-content :link, .book-content a {
  color: #8db2e5 !important;
}

.book-content img, .book-content img[src] {
z-index: 1;
filter: brightness(0.85) !important;
background-color: initial !important;
}

.reader-container {
  color: #dcdcdc !important;
  background-image: none !important;
  background-color: black !important;
}

.book-content *:not(code), .book-content *:not(a) {
    background-color: black;
    box-shadow: none;
    text-shadow: none;
    border-radius: unset;
    color: #dcdcdc !important;
}
  
.book-content :visited, .book-content :visited *, .book-content :visited *[class] {color: rgb(211, 138, 138) !important}
.book-content :link:not(cite), :link .book-content *:not(cite) {color: #8db2e5 !important}
`;var je=`
:root .brtheme-dark {
  /* General */
  --color-scheme: dark;
  --bs-body-color: #292929;
  --hr-color: rgba(239, 239, 239, 0.125);
  --accent-bg-color: rgba(1, 4, 9, 0.5);
  --accent-text-color: lightgrey;
  --body-text-color: #efefef;
  --btn-icon-filter: invert(1) grayscale(100%) brightness(200%);

  /* Drawer */
  --drawer-bg-color: #292929;
  --drawer-text-color: white;
  --drawer-pagination-horizontal-rule: inset 0 -1px 0 rgb(255 255 255 / 20%);
  --drawer-pagination-border: 1px solid rgb(0 0 0 / 13%);

  /* Accordion */
  --accordion-surface-bg-color: #292929;
  --accordion-header-text-color: var(--body-text-color);
  --accordion-header-bg-color: transparent;
  --accordion-body-bg-color: var(--accordion-surface-bg-color);
  --accordion-body-border-color: rgba(239, 239, 239, 0.125);
  --accordion-body-text-color: var(--body-text-color);
  --accordion-header-collapsed-text-color: var(--body-text-color);
  --accordion-header-collapsed-bg-color: transparent;
  --accordion-button-focus-border-color: unset;
  --accordion-button-focus-box-shadow: unset;
  --accordion-active-body-bg-color: var(--accordion-surface-bg-color);
  --accordion-body-box-shadow: none;
  --accordion-hairline-color: rgba(255, 255, 255, 0.08);
  --accordion-subtitle-text-color: #8b95a5;
  --accordion-meta-text-color: rgba(255, 255, 255, 0.45);
  --accordion-chevron-color: #8b95a5;
  --accordion-chevron-hover-color: var(--body-text-color);
  --accordion-border-hover-color: rgba(255, 255, 255, 0.2);
  --accordion-radius: var(--card-border-radius);
  --accordion-gap: 0.75rem;

  /* Buttons */
    --btn-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-primary-text-color: white;
    --btn-primary-bg-color: var(--primary-color);
    --btn-primary-border-color: var(--primary-color);
    --btn-primary-hover-text-color: white;
    --btn-primary-hover-bg-color: var(--primary-color-darker-shade);
    --btn-primary-hover-border-color: var(--primary-color-darker-shade);
    --btn-alt-bg-color: #424c72;
    --btn-alt-border-color: #444f75;
    --btn-alt-hover-bg-color: #3b4466;
    --btn-alt-focus-bg-color: #343c59;
    --btn-alt-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-fa-icon-color: white;
    --btn-disabled-bg-color: #343a40;
    --btn-disabled-text-color: white;
    --btn-disabled-border-color: #6c757d;

    /* Inputs */
    --input-bg-color: #343a40;
    --input-bg-readonly-color: #434648;
    --input-focused-border-color: #ccc;
    --input-text-color: #fff;
    --input-placeholder-color: #aeaeae;
    --input-border-color: #ccc;
    --input-focus-boxshadow-color: rgb(255 255 255 / 50%);

    /* Nav (Tabs) */
    --nav-tab-border-color: rgba(44, 118, 88, 0.7);
    --nav-tab-text-color: var(--body-text-color);
    --nav-tab-bg-color: var(--primary-color);
    --nav-tab-hover-border-color: var(--primary-color);
    --nav-tab-active-text-color: white;
    --nav-tab-border-hover-color: transparent;
    --nav-tab-hover-text-color: var(--body-text-color);
    --nav-tab-hover-bg-color: transparent;
    --nav-tab-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-border-bottom: rgba(44, 118, 88, 0.7);
    --nav-tab-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-bottom: var(--bs-body-bg);
    --nav-tab-hover-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-active-hover-bg-color: var(--primary-color);
    --nav-link-bg-color: var(--primary-color);
    --nav-link-active-text-color: white;
    --nav-link-text-color: white;

    /* Checkboxes/Switch */
    --checkbox-checked-bg-color: var(--primary-color);
    --checkbox-border-color: var(--input-focused-border-color);
    --checkbox-focus-border-color: var(--primary-color);
    --checkbox-focus-boxshadow-color: rgb(255 255 255 / 50%);



    /* Reading Bar */
    --br-actionbar-button-text-color: white;
    --br-actionbar-button-hover-border-color: #6c757d;
    --br-actionbar-bg-color: black;
    
}



.book-content *:not(input), .book-content *:not(select), .book-content *:not(code), .book-content *:not(:link), .book-content *:not(.ngx-toastr) {
  color: #dcdcdc !important;
}

.book-content code {
  color: #e83e8c !important;
}

.book-content :link, .book-content a {
  color: #8db2e5 !important;
}

.book-content img, .book-content img[src] {
z-index: 1;
filter: brightness(0.85) !important;
background-color: initial !important;
}

.reader-container {
  color: #dcdcdc !important;
  background-image: none !important;
  background-color: #292929 !important;
}

.book-content *:not(code), .book-content *:not(a) {
    background-color: #292929;
    box-shadow: none;
    text-shadow: none;
    border-radius: unset;
    color: #dcdcdc !important;
}
  
.book-content :visited, .book-content :visited *, .book-content :visited *[class] {color: rgb(211, 138, 138) !important}
.book-content :link:not(cite), :link .book-content *:not(cite) {color: #8db2e5 !important}

`;var Ge=`
  :root .brtheme-white {
    --drawer-text-color: white;
    --br-actionbar-bg-color: white;
    --bs-btn-active-color: black;
    --progress-bg-color: rgb(222, 226, 230);

    /* General */
    --color-scheme: light;
    --bs-body-color: black;
    --hr-color: rgba(239, 239, 239, 0.125);
    --accent-bg-color: rgba(1, 4, 9, 0.5);
    --accent-text-color: lightgrey;
    --body-text-color: black;
    --btn-icon-filter: invert(1) grayscale(100%) brightness(200%);

    /* Drawer */
    --drawer-bg-color: white;
    --drawer-text-color: black;
    --drawer-pagination-horizontal-rule: inset 0 -1px 0 rgb(255 255 255 / 20%);
    --drawer-pagination-border: 1px solid rgb(0 0 0 / 13%);


    /* Accordion */
    --accordion-surface-bg-color: white;
    --accordion-header-text-color: var(--body-text-color);
    --accordion-header-bg-color: transparent;
    --accordion-body-bg-color: var(--accordion-surface-bg-color);
    --accordion-body-border-color: rgba(0, 0, 0, 0.125);
    --accordion-body-text-color: var(--body-text-color);
    --accordion-header-collapsed-text-color: var(--body-text-color);
    --accordion-header-collapsed-bg-color: transparent;
    --accordion-button-focus-border-color: unset;
    --accordion-button-focus-box-shadow: unset;
    --accordion-active-body-bg-color: var(--accordion-surface-bg-color);
    --accordion-body-box-shadow: none;
    --accordion-hairline-color: rgba(0, 0, 0, 0.08);
    --accordion-subtitle-text-color: #6c757d;
    --accordion-meta-text-color: rgba(0, 0, 0, 0.45);
    --accordion-chevron-color: #6c757d;
    --accordion-chevron-hover-color: var(--body-text-color);
    --accordion-border-hover-color: rgba(0, 0, 0, 0.2);
    --accordion-radius: var(--card-border-radius);
    --accordion-gap: 0.75rem;

    /* Buttons */
    --btn-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-primary-text-color: white;
    --btn-primary-bg-color: var(--primary-color);
    --btn-primary-border-color: var(--primary-color);
    --btn-primary-hover-text-color: white;
    --btn-primary-hover-bg-color: var(--primary-color-darker-shade);
    --btn-primary-hover-border-color: var(--primary-color-darker-shade);
    --btn-alt-bg-color: #424c72;
    --btn-alt-border-color: #444f75;
    --btn-alt-hover-bg-color: #3b4466;
    --btn-alt-focus-bg-color: #343c59;
    --btn-alt-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-fa-icon-color: black;
    --btn-disabled-bg-color: #343a40;
    --btn-disabled-text-color: #efefef;
    --btn-disabled-border-color: #6c757d;
    --btn-outline-primary-text-color: black;

    /* Inputs */
    --input-bg-color: white;
    --input-bg-readonly-color: white;
    --input-focused-border-color: #ccc;
    --input-text-color: black;
    --input-placeholder-color: black;
    --input-border-color: #ccc;
    --input-focus-boxshadow-color: rgb(255 255 255 / 50%);

    /* Nav (Tabs) */
    --nav-tab-border-color: rgba(44, 118, 88, 0.7);
    --nav-tab-text-color: var(--body-text-color);
    --nav-tab-bg-color: var(--primary-color);
    --nav-tab-hover-border-color: var(--primary-color);
    --nav-tab-active-text-color: white;
    --nav-tab-border-hover-color: transparent;
    --nav-tab-hover-text-color: var(--body-text-color);
    --nav-tab-hover-bg-color: transparent;
    --nav-tab-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-border-bottom: rgba(44, 118, 88, 0.7);
    --nav-tab-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-bottom: var(--bs-body-bg);
    --nav-tab-hover-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-active-hover-bg-color: var(--primary-color);
    --nav-link-bg-color: var(--primary-color);
    --nav-link-active-text-color: white;
    --nav-link-text-color: white;



  /* Reading Bar */
  --br-actionbar-button-text-color: black;
  --br-actionbar-button-hover-border-color: #6c757d;
  --br-actionbar-bg-color: white;

  /* Drawer */
  --drawer-pagination-horizontal-rule: inset 0 -1px 0 rgb(0 0 0 / 13%);
  --drawer-pagination-border: 1px solid rgb(0 0 0 / 13%);
}

.reader-container {
  color: black !important;
  background-image: none !important;
  background-color: white !important;
}


.book-content *:not(input), .book-content *:not(select), .book-content *:not(code), .book-content *:not(:link), .book-content *:not(.ngx-toastr) {
  color: black;
}

.book-content code {
  color: #e83e8c !important;
}

.book-content :link, .book-content a {
  color: #8db2e5 !important;
}

.book-content img, .book-content img[src] {
  z-index: 1;
  background-color: initial !important;
}


.book-content *:not(code), .book-content *:not(a) {
  background-color: white;
  box-shadow: none;
  text-shadow: none;
  border-radius: unset;
  color: #dcdcdc;
}

.book-content :visited, .book-content :visited *, .book-content :visited *[class] {
  color: rgb(240, 50, 50) !important;
}
.book-content :link:not(cite), :link .book-content *:not(cite) {
  color: #00f !important;
}

.btn-check:checked + .btn {
  color: white;
  background-color: var(--primary-color);
}

`;var Ke=`
  :root .brtheme-paper {
    --drawer-text-color: white;
    --br-actionbar-bg-color: white;
    --bs-btn-active-color: black;
    --progress-bg-color: rgb(222, 226, 230);

    /* General */
    --color-scheme: light;
    --bs-body-color: black;
    --hr-color: rgba(239, 239, 239, 0.125);
    --accent-bg-color: rgba(1, 4, 9, 0.5);
    --accent-text-color: lightgrey;
    --body-text-color: black;
    --btn-icon-filter: invert(1) grayscale(100%) brightness(200%);

    /* Drawer */
    --drawer-bg-color: #F1E4D5;
    --drawer-text-color: black;
    --drawer-pagination-horizontal-rule: inset 0 -1px 0 rgb(255 255 255 / 20%);


    /* Accordion */
    --accordion-surface-bg-color: #F1E4D5;
    --accordion-header-text-color: var(--body-text-color);
    --accordion-header-bg-color: transparent;
    --accordion-body-bg-color: var(--accordion-surface-bg-color);
    --accordion-body-border-color: rgba(0, 0, 0, 0.125);
    --accordion-body-text-color: var(--body-text-color);
    --accordion-header-collapsed-text-color: var(--body-text-color);
    --accordion-header-collapsed-bg-color: transparent;
    --accordion-button-focus-border-color: unset;
    --accordion-button-focus-box-shadow: unset;
    --accordion-active-body-bg-color: var(--accordion-surface-bg-color);
    --accordion-body-box-shadow: none;
    --accordion-hairline-color: rgba(0, 0, 0, 0.08);
    --accordion-subtitle-text-color: rgba(0, 0, 0, 0.55);
    --accordion-meta-text-color: rgba(0, 0, 0, 0.45);
    --accordion-chevron-color: rgba(0, 0, 0, 0.5);
    --accordion-chevron-hover-color: var(--body-text-color);
    --accordion-border-hover-color: rgba(0, 0, 0, 0.25);
    --accordion-radius: var(--card-border-radius);
    --accordion-gap: 0.75rem;

    /* Buttons */
    --btn-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-primary-text-color: white;
    --btn-primary-bg-color: var(--primary-color);
    --btn-primary-border-color: var(--primary-color);
    --btn-primary-hover-text-color: white;
    --btn-primary-hover-bg-color: var(--primary-color-darker-shade);
    --btn-primary-hover-border-color: var(--primary-color-darker-shade);
    --btn-alt-bg-color: #424c72;
    --btn-alt-border-color: #444f75;
    --btn-alt-hover-bg-color: #3b4466;
    --btn-alt-focus-bg-color: #343c59;
    --btn-alt-focus-boxshadow-color: rgb(255 255 255 / 50%);
    --btn-fa-icon-color: black;
    --btn-disabled-bg-color: #343a40;
    --btn-disabled-text-color: #efefef;
    --btn-disabled-border-color: #6c757d;
    --btn-outline-primary-text-color: black;

    /* Inputs */
    --input-bg-color: white;
    --input-bg-readonly-color: #F1E4D5;
    --input-focused-border-color: #ccc;
    --input-placeholder-color: black;
    --input-border-color: #ccc;
    --input-text-color: black;
    --input-focus-boxshadow-color: rgb(255 255 255 / 50%);

    /* Nav (Tabs) */
    --nav-tab-border-color: rgba(44, 118, 88, 0.7);
    --nav-tab-text-color: var(--body-text-color);
    --nav-tab-bg-color: var(--primary-color);
    --nav-tab-hover-border-color: var(--primary-color);
    --nav-tab-active-text-color: white;
    --nav-tab-border-hover-color: transparent;
    --nav-tab-hover-text-color: var(--body-text-color);
    --nav-tab-hover-bg-color: transparent;
    --nav-tab-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-border-bottom: rgba(44, 118, 88, 0.7);
    --nav-tab-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-top: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-left: rgba(44, 118, 88, 0.7);
    --nav-tab-hover-border-bottom: var(--bs-body-bg);
    --nav-tab-hover-border-right: rgba(44, 118, 88, 0.7);
    --nav-tab-active-hover-bg-color: var(--primary-color);
    --nav-link-bg-color: var(--primary-color);
    --nav-link-active-text-color: white;
    --nav-link-text-color: white;

  /* Reading Bar */
  --br-actionbar-button-hover-border-color: #6c757d;
  --br-actionbar-bg-color: #F1E4D5;

  /* Drawer */
  --drawer-pagination-horizontal-rule: inset 0 -1px 0 rgb(0 0 0 / 13%);

  /* Custom variables */
  --theme-bg-color: #fff3c9;

  --bs-secondary-bg: darkgrey;
}

.reader-container {
  color: black !important;
  background-color: var(--theme-bg-color) !important;
  background: url("assets/images/paper-bg.png");
}

.book-content *:not(input), .book-content *:not(select), .book-content *:not(code), .book-content *:not(:link), .book-content *:not(.ngx-toastr) {
  color: var(--bs-body-color) !important;
}

.book-content code {
  color: #e83e8c !important;
}

// KDB has a reboot style so for lighter themes, this is needed
.book-content kbd {
  background-color: transparent;
}

.book-content :link, .book-content a {
  color: #8db2e5 !important;
}

.book-content img, .book-content img[src] {
  z-index: 1;
  background-color: initial !important;
}


.book-content *:not(code), .book-content *:not(a), .book-content *:not(kbd) {
    //background-color: #F1E4D5;
    box-shadow: none;
    text-shadow: none;
    border-radius: unset;
    color: #dcdcdc !important;
}

.book-content :visited, .book-content :visited *, .book-content :visited *[class] {
  color: rgb(240, 50, 50) !important;
}
.book-content :link:not(cite), :link .book-content *:not(cite) {
  color: #00f !important;
}

.btn-check:checked + .btn {
  color: white;
  background-color: var(--primary-color);
}

.reader-container.column-layout-2::before {
  content: "";
  position: absolute;
  top: 0;
  left: 50%;
  height: 100%;
  box-shadow: 0px 0px 34.38px 5px rgba(0, 0, 0, 0.43), 0px 0px 6.28px 2px rgba(0, 0, 0, 0.43), 0px 0px 15.7px 4px rgba(0, 0, 0, 0.43), 0px 0px 1.57px 0.3px rgba(0, 0, 0, 0.43);
}

`;var Ze=["container"],et=["ngbAccordionBody",""],tt=["*"],ot=(()=>{class t{constructor(){this._ngbConfig=u(Ce),this.closeOthers=!1,this.destroyOnHide=!0}get animation(){return this._animation??this._ngbConfig.animation}set animation(e){this._animation=e}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275prov=ne({token:t,factory:t.\u0275fac,providedIn:"root"})}}return t})(),rt=0,qe=(()=>{class t{constructor(){this._item=u(T),this._viewRef=null,this.elementRef=u(ae)}ngAfterContentChecked(){this._bodyTpl&&(this._item._shouldBeInDOM?this._createViewIfNotExists():this._destroyViewIfExists())}ngOnDestroy(){this._destroyViewIfExists()}_destroyViewIfExists(){this._viewRef?.destroy(),this._viewRef=null}_createViewIfNotExists(){this._viewRef||(this._viewRef=this._vcr.createEmbeddedView(this._bodyTpl),this._viewRef.detectChanges())}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275cmp=N({type:t,selectors:[["","ngbAccordionBody",""]],contentQueries:function(o,n,p){if(o&1&&H(p,le,7),o&2){let v;E(v=A())&&(n._bodyTpl=v.first)}},viewQuery:function(o,n){if(o&1&&he(Ze,7,de),o&2){let p;E(p=A())&&(n._vcr=p.first)}},hostAttrs:[1,"accordion-body"],attrs:et,ngContentSelectors:tt,decls:3,vars:0,consts:[["container",""]],template:function(o,n){o&1&&(me(),pe(0,null,0),ge(2))},encapsulation:2})}}return t})(),J=(()=>{class t{constructor(){this.item=u(T),this.ngbCollapse=u(Q)}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275dir=R({type:t,selectors:[["","ngbAccordionCollapse",""]],hostAttrs:["role","region",1,"accordion-collapse"],hostVars:2,hostBindings:function(o,n){o&2&&(P("id",n.item.collapseId),C("aria-labelledby",n.item.toggleId))},exportAs:["ngbAccordionCollapse"],features:[L([Q])]})}}return t})(),nt=(()=>{class t{constructor(){this.item=u(T),this.accordion=u(W)}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275dir=R({type:t,selectors:[["","ngbAccordionToggle",""]],hostVars:5,hostBindings:function(o,n){o&1&&y("click",function(){return!n.item.disabled&&n.accordion.toggle(n.item.id)}),o&2&&(P("id",n.item.toggleId),C("aria-controls",n.item.collapseId)("aria-expanded",!n.item.collapsed),K("collapsed",n.item.collapsed))}})}}return t})(),Ue=(()=>{class t{constructor(){this.item=u(T)}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275dir=R({type:t,selectors:[["button","ngbAccordionButton",""]],hostAttrs:["type","button",1,"accordion-button"],hostVars:1,hostBindings:function(o,n){o&2&&P("disabled",n.item.disabled)},features:[L([nt])]})}}return t})(),Ye=(()=>{class t{constructor(){this.item=u(T)}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275dir=R({type:t,selectors:[["","ngbAccordionHeader",""]],hostAttrs:["role","heading",1,"accordion-header"],hostVars:2,hostBindings:function(o,n){o&2&&K("collapsed",n.item.collapsed)}})}}return t})(),T=(()=>{class t{constructor(){this._accordion=u(W),this._cd=u($),this._destroyRef=u(ie),this._collapsed=!0,this._id=`ngb-accordion-item-${rt++}`,this._collapseAnimationRunning=!1,this.disabled=!1,this.show=new x,this.shown=new x,this.hide=new x,this.hidden=new x}set id(e){Se(e)&&e!==""&&(this._id=e)}set destroyOnHide(e){this._destroyOnHide=e}get destroyOnHide(){return this._destroyOnHide===void 0?this._accordion.destroyOnHide:this._destroyOnHide}set collapsed(e){e?this.collapse():this.expand()}get collapsed(){return this._collapsed}get id(){return`${this._id}`}get toggleId(){return`${this.id}-toggle`}get collapseId(){return`${this.id}-collapse`}get _shouldBeInDOM(){return!this.collapsed||this._collapseAnimationRunning||!this.destroyOnHide}ngAfterContentInit(){let{ngbCollapse:e}=this._collapse;e.animation=!1,e.collapsed=this.collapsed,e.animation=this._accordion.animation,e.hidden.pipe(Y(this._destroyRef)).subscribe(()=>{this._collapseAnimationRunning=!1,this.hidden.emit(),this._accordion.hidden.emit(this.id),this._cd.markForCheck()}),e.shown.pipe(Y(this._destroyRef)).subscribe(()=>{this.shown.emit(),this._accordion.shown.emit(this.id),this._cd.markForCheck()})}toggle(){this.collapsed=!this.collapsed}expand(){if(this.collapsed){if(!this._accordion._ensureCanExpand(this))return;this._collapsed=!1,this._cd.markForCheck(),this._cd.detectChanges(),this.show.emit(),this._accordion.show.emit(this.id),this._collapse.ngbCollapse.animation=this._accordion.animation,this._collapse.ngbCollapse.collapsed=!1}}collapse(){this.collapsed||(this._collapsed=!0,this._collapseAnimationRunning=!0,this._cd.markForCheck(),this.hide.emit(),this._accordion.hide.emit(this.id),this._collapse.ngbCollapse.animation=this._accordion.animation,this._collapse.ngbCollapse.collapsed=!0)}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275dir=R({type:t,selectors:[["","ngbAccordionItem",""]],contentQueries:function(o,n,p){if(o&1&&H(p,J,7),o&2){let v;E(v=A())&&(n._collapse=v.first)}},hostAttrs:[1,"accordion-item"],hostVars:1,hostBindings:function(o,n){o&2&&P("id",n.id)},inputs:{id:[0,"ngbAccordionItem","id"],destroyOnHide:"destroyOnHide",disabled:"disabled",collapsed:"collapsed"},outputs:{show:"show",shown:"shown",hide:"hide",hidden:"hidden"},exportAs:["ngbAccordionItem"]})}}return t})(),W=(()=>{class t{constructor(){this._config=u(ot),this._anItemWasAlreadyExpandedDuringInitialisation=!1,this.animation=this._config.animation,this.closeOthers=this._config.closeOthers,this.destroyOnHide=this._config.destroyOnHide,this.show=new x,this.shown=new x,this.hide=new x,this.hidden=new x}toggle(e){this._getItem(e)?.toggle()}expand(e){this._getItem(e)?.expand()}expandAll(){this._items&&(this.closeOthers?this._items.find(e=>!e.collapsed)||this._items.first.expand():this._items.forEach(e=>e.expand()))}collapse(e){this._getItem(e)?.collapse()}collapseAll(){this._items?.forEach(e=>e.collapse())}isExpanded(e){let o=this._getItem(e);return o?!o.collapsed:!1}_ensureCanExpand(e){return this.closeOthers?this._items?(this._items.find(o=>!o.collapsed&&e!==o)?.collapse(),!0):this._anItemWasAlreadyExpandedDuringInitialisation?!1:(this._anItemWasAlreadyExpandedDuringInitialisation=!0,!0):!0}_getItem(e){return this._items?.find(o=>o.id===e)}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275dir=R({type:t,selectors:[["","ngbAccordion",""]],contentQueries:function(o,n,p){if(o&1&&H(p,T,4),o&2){let v;E(v=A())&&(n._items=v)}},hostAttrs:[1,"accordion"],inputs:{animation:"animation",closeOthers:"closeOthers",destroyOnHide:"destroyOnHide"},outputs:{show:"show",shown:"shown",hide:"hide",hidden:"hidden"},exportAs:["ngbAccordion"]})}}return t})();var it=t=>({name:t}),at=t=>({active:t}),ct=t=>({"background-color":t}),Je=(t,h)=>h.name;function lt(t,h){if(t&1&&(i(0,"option",26),c(1),ve(2,"titlecase"),a()),t&2){let e=h.$implicit;l("value",e.name),r(),m(fe(2,2,e.name))}}function dt(t,h){if(t&1){let e=M();i(0,"div",21)(1,"div",22)(2,"div",23)(3,"label",24),c(4),a(),i(5,"select",25),j(6,lt,3,4,"option",26,Je),a()()(),i(8,"div",27)(9,"label",28),c(10),a(),i(11,"span",29),s(12,"i",30)(13,"input",31)(14,"i",32),a()(),i(15,"div",27)(16,"label",33),c(17),a(),i(18,"span",29),c(19),s(20,"input",34),c(21),a()(),i(22,"div",27)(23,"label",35),c(24),a(),i(25,"span",29),s(26,"i",36)(27,"input",37)(28,"i",38),a()(),i(29,"div",39)(30,"button",40),y("click",function(){k(e);let n=d(3);return w(n.resetSettings())}),c(31),a()()()}if(t&2){let e,o=d().$implicit,n=d(2);r(4),m(o("font-family-label")),r(2),G(n.epubFonts()),r(4),m(o("font-size-label")),r(3),l("ngbTooltip",n.pageStyles()["font-size"]),r(4),m(o("line-spacing-label")),r(2),f(" ",o("line-spacing-min-label")," "),r(),l("ngbTooltip",n.pageStyles()["line-height"]),r(),f(" ",o("line-spacing-max-label")," "),r(3),m(o("margin-label")),r(3),l("ngbTooltip",((e=n.settingsForm.get("bookReaderMargin"))==null?null:e.value)+"%"),r(4),m(o("reset-to-defaults"))}}function st(t,h){if(t&1&&c(0),t&2){let e=d(2).$implicit;m(e("writing-style-tooltip"))}}function bt(t,h){if(t&1&&c(0),t&2){let e=d(2).$implicit;m(e("tap-to-paginate-tooltip"))}}function pt(t,h){if(t&1&&c(0),t&2){let e=d(2).$implicit;m(e("immersive-mode-tooltip"))}}function mt(t,h){if(t&1&&c(0),t&2){let e=d(2).$implicit;m(e("disable-bookmark-icon-tooltip"))}}function gt(t,h){if(t&1&&c(0),t&2){let e=d(2).$implicit;m(e("fullscreen-tooltip"))}}function ht(t,h){if(t&1&&(i(0,"span",69),c(1),a()),t&2){let e=d(2).$implicit,o=d(2);r(),m(o.isFullscreen()?e("exit"):e("enter"))}}function ut(t,h){if(t&1&&s(0,"span",78),t&2){let e=d(2).$implicit;l("innerHTML",e("layout-mode-tooltip"),ce)}}function vt(t,h){if(t&1&&(s(0,"input",79),i(1,"label",80),c(2),a()),t&2){let e=d(2).$implicit,o=d(2);l("value",o.BookPageLayoutMode.Column2),r(2),m(e("layout-mode-option-2col"))}}function ft(t,h){if(t&1){let e=M();i(0,"div",41)(1,"label",42),c(2),a(),i(3,"button",43),y("click",function(){k(e);let n=d(3);return w(n.toggleReadingDirection())}),s(4,"i",44),i(5,"span",45),c(6),a()()(),i(7,"div",46)(8,"label",47),c(9),s(10,"i",48),a(),_(11,st,1,1,"ng-template",null,1,D),i(13,"span",49),I(14,50),a(),i(15,"button",51),y("click",function(){k(e);let n=d(3);return w(n.toggleWritingStyle())}),s(16,"i",44),i(17,"span",45),c(18),a()()(),i(19,"div",41)(20,"label",52),c(21),s(22,"i",53),a(),_(23,bt,1,1,"ng-template",null,2,D),i(25,"span",54),I(26,50),a(),i(27,"div",55),s(28,"input",56),i(29,"label"),c(30),a()()(),i(31,"div",41)(32,"label",57),c(33),s(34,"i",58),a(),_(35,pt,1,1,"ng-template",null,3,D),i(37,"span",59),I(38,50),a(),i(39,"div",55),s(40,"input",60),i(41,"label"),c(42),a()()(),i(43,"div",41)(44,"label",61),c(45),s(46,"i",62),a(),_(47,mt,1,1,"ng-template",null,4,D),i(49,"span",63),I(50,50),a(),i(51,"div",55),s(52,"input",64),i(53,"label"),c(54),a()()(),i(55,"div",41)(56,"label",65),c(57),s(58,"i",66),a(),_(59,gt,1,1,"ng-template",null,5,D),i(61,"span",67),I(62,50),a(),i(63,"button",68),y("click",function(){k(e);let n=d(3);return w(n.toggleFullscreen())}),s(64,"i",44),O(65,ht,2,1,"span",69),a()(),i(66,"div",22)(67,"label",70),c(68),s(69,"i",71),a(),_(70,ut,1,1,"ng-template",null,6,D),i(72,"span",72),I(73,50),a(),s(74,"br"),i(75,"div",73),s(76,"input",74),i(77,"label",75),c(78),a(),s(79,"input",76),i(80,"label",77),c(81),a(),O(82,vt,3,2),a()()}if(t&2){let e,o,n,p,v=S(12),X=S(24),Z=S(36),ee=S(48),te=S(60),oe=S(71),b=d().$implicit,g=d(2);r(2),m(b("reading-direction-label")),r(),l("title",q(g.readingDirectionModel()===g.ReadingDirection.LeftToRight?b("left-to-right"):b("right-to-left"))),r(),V(U("fa ",g.readingDirectionModel()===g.ReadingDirection.LeftToRight?"fa-arrow-right":"fa-arrow-left"," ")),r(2),f("\xA0",g.readingDirectionModel()===g.ReadingDirection.LeftToRight?b("left-to-right"):b("right-to-left")),r(3),m(b("writing-style-label")),r(),l("ngbTooltip",v),r(4),l("ngTemplateOutlet",v),r(),l("title",q(g.writingStyleModel()===g.WritingStyle.Horizontal?b("horizontal"):b("vertical"))),r(),V(U("fa ",g.writingStyleModel()===g.WritingStyle.Horizontal?"fa-arrows-left-right":"fa-arrows-up-down")),r(2),f(" ",g.writingStyleModel()===g.WritingStyle.Horizontal?b("horizontal"):b("vertical")),r(3),m(b("tap-to-paginate-label")),r(),l("ngbTooltip",X),r(4),l("ngTemplateOutlet",X),r(4),f("",(e=g.settingsForm.get("bookReaderTapToPaginate"))!=null&&e.value?b("on"):b("off")," "),r(3),m(b("immersive-mode-label")),r(),l("ngbTooltip",Z),r(4),l("ngTemplateOutlet",Z),r(4),f("",(o=g.settingsForm.get("bookReaderImmersiveMode"))!=null&&o.value?b("on"):b("off")," "),r(3),m(b("disable-bookmark-icon-label")),r(),l("ngbTooltip",ee),r(4),l("ngTemplateOutlet",ee),r(4),f("",(n=g.settingsForm.get("bookReaderDisableBookmarkIcon"))!=null&&n.value?b("on"):b("off")," "),r(3),m(b("fullscreen-label")),r(),l("ngbTooltip",te),r(4),l("ngTemplateOutlet",te),r(2),V(ue("fa ",g.isFullscreen()?"fa-compress-alt":"fa-expand-alt"," ",g.isFullscreen()?"icon-primary-color":"")),r(),B((p=g.activeTheme())!=null&&p.isDarkTheme?65:-1),r(3),m(b("layout-mode-label")),r(),l("ngbTooltip",oe),r(4),l("ngTemplateOutlet",oe),r(2),C("aria-label",b("layout-mode-label")),r(),l("value",g.BookPageLayoutMode.Default),r(2),m(b("layout-mode-option-scroll")),r(),l("value",g.BookPageLayoutMode.Column1),r(2),m(b("layout-mode-option-1col")),r(),B(g.writingStyleModel()===g.WritingStyle.Horizontal?82:-1)}}function _t(t,h){if(t&1){let e=M();i(0,"button",82),y("click",function(){let n=k(e).$implicit,p=d(4);return w(p.setTheme(n.name))}),s(1,"div",83),c(2),a()}if(t&2){let e,o=h.$implicit,n=d(2).$implicit,p=d(2);l("ngClass",z(3,at,((e=p.activeTheme())==null?null:e.name)===o.name)),r(),l("ngStyle",z(5,ct,o.colorHash)),r(),f(" ",n(o.translationKey)," ")}}function yt(t,h){if(t&1&&(i(0,"div",22),j(1,_t,3,7,"button",81,Je),a()),t&2){let e=d(3);r(),G(e.themes)}}function xt(t,h){if(t&1){let e=M();se(0),i(1,"form",8)(2,"div",9,0)(4,"div",10)(5,"h2",11)(6,"button",12),c(7),s(8,"i",13),a()(),i(9,"div",14)(10,"div",15),_(11,dt,32,10,"ng-template"),a()()(),i(12,"div",16)(13,"h2",11)(14,"button",12),c(15),s(16,"i",13),a()(),i(17,"div",14)(18,"div",15),_(19,ft,83,45,"ng-template"),a()()(),i(20,"div",17)(21,"h2",11)(22,"button",12),c(23),s(24,"i",13),a()(),i(25,"div",14)(26,"div",15),_(27,yt,3,0,"ng-template"),a()()(),i(28,"div",18)(29,"button",19),y("click",function(){k(e);let n=d(2);return w(n.updateParentPref())}),c(30),a(),i(31,"button",20),y("click",function(){k(e);let n=d(2);return w(n.createNewProfileFromImplicit())}),c(32),a()()()(),be()}if(t&2){let e,o=h.$implicit,n=S(3),p=d(2);r(),l("formGroup",p.settingsForm),r(),l("closeOthers",!1),r(2),l("collapsed",!1),r(2),C("aria-expanded",n.isExpanded("general-panel")),r(),f(" ",o("general-settings-title")," "),r(5),l("title",o("general-settings-title"))("collapsed",!1),r(2),C("aria-expanded",n.isExpanded("reader-panel")),r(),f(" ",o("reader-settings-title")," "),r(5),l("title",o("color-theme-title"))("collapsed",!1),r(2),C("aria-expanded",n.isExpanded("color-panel")),r(),f(" ",o("color-theme-title")," ");let v=p.currentReadingProfile();r(6),l("disabled",!v||v.kind!==p.ReadingProfileKind.Implicit||!p.hasParentProfile()),r(),f(" ",o("update-parent",z(18,it,((e=p.parentReadingProfile())==null?null:e.name)||o("loading")))," "),r(),l("ngbTooltip",o("create-new-tooltip"))("disabled",!p.canPromoteProfile()),r(),f(" ",o("create-new")," ")}}function kt(t,h){t&1&&_(0,xt,33,20,"ng-container",7),t&2&&l("translocoPrefix","reader-settings")}var so=[{name:"Dark",colorHash:"#292929",isDarkTheme:!0,isDefault:!0,provider:F.System,selector:"brtheme-dark",content:je,translationKey:"theme-dark"},{name:"Black",colorHash:"#000000",isDarkTheme:!0,isDefault:!1,provider:F.System,selector:"brtheme-black",content:Le,translationKey:"theme-black"},{name:"White",colorHash:"#FFFFFF",isDarkTheme:!1,isDefault:!1,provider:F.System,selector:"brtheme-white",content:Ge,translationKey:"theme-white"},{name:"Paper",colorHash:"#F1E4D5",isDarkTheme:!1,isDefault:!1,provider:F.System,selector:"brtheme-paper",content:Ke,translationKey:"theme-paper"}],bo=(()=>{class t{constructor(){this.cdRef=u($),this.themes=[],this.ReadingProfileKind=We,this.WritingStyle=Qe,this.ReadingDirection=$e,this.BookPageLayoutMode=ze}ngOnInit(){return re(this,null,function*(){this.pageStyles=this.readerSettingsService.pageStyles,this.readingDirectionModel=this.readerSettingsService.readingDirection,this.writingStyleModel=this.readerSettingsService.writingStyle,this.activeTheme=this.readerSettingsService.activeTheme,this.layoutMode=this.readerSettingsService.layoutMode,this.immersiveMode=this.readerSettingsService.immersiveMode,this.clickToPaginate=this.readerSettingsService.clickToPaginate,this.isFullscreen=this.readerSettingsService.isFullscreen,this.canPromoteProfile=this.readerSettingsService.canPromoteProfile,this.hasParentProfile=this.readerSettingsService.hasParentProfile,this.parentReadingProfile=this.readerSettingsService.parentReadingProfile,this.currentReadingProfile=this.readerSettingsService.currentReadingProfile,this.epubFonts=this.readerSettingsService.epubFonts,this.themes=this.readerSettingsService.getThemes(),this.readerSettingsService.getCurrentReadingProfile()||(yield this.readerSettingsService.initialize(this.libraryId,this.seriesId,this.readingProfile)),this.settingsForm=this.readerSettingsService.getSettingsForm(),this.cdRef.markForCheck()})}resetSettings(){this.readerSettingsService.resetSettings()}setTheme(e,o=!0){this.readerSettingsService.setTheme(e,o)}toggleReadingDirection(){this.readerSettingsService.toggleReadingDirection()}toggleWritingStyle(){this.readerSettingsService.toggleWritingStyle()}toggleFullscreen(){this.readerSettingsService.toggleFullscreen()}updateParentPref(){this.readerSettingsService.updateParentProfile()}createNewProfileFromImplicit(){this.readerSettingsService.createNewProfileFromImplicit()}static{this.\u0275fac=function(o){return new(o||t)}}static{this.\u0275cmp=N({type:t,selectors:[["app-reader-settings"]],inputs:{libraryId:"libraryId",seriesId:"seriesId",readingProfile:"readingProfile",readerSettingsService:"readerSettingsService"},decls:1,vars:1,consts:[["acc","ngbAccordion"],["writingStyleTooltip",""],["tapPaginationTooltip",""],["immersiveModeTooltip",""],["disableBookmarkIconToolTip",""],["fullscreenTooltip",""],["layoutTooltip",""],[4,"transloco","translocoPrefix"],[3,"formGroup"],["ngbAccordion","",3,"closeOthers"],["ngbAccordionItem","","id","general-panel","title","General Settings",3,"collapsed"],["ngbAccordionHeader","",1,"accordion-header"],["ngbAccordionButton","","type","button","aria-controls","collapseOne",1,"accordion-button"],["aria-hidden","true",1,"fas","fa-chevron-up"],["ngbAccordionCollapse",""],["ngbAccordionBody",""],["ngbAccordionItem","","id","reader-panel",3,"title","collapsed"],["ngbAccordionItem","","id","color-panel",3,"title","collapsed"],[1,"row","g-0","mt-2"],[1,"btn","btn-primary","col-12","mb-2",3,"click","disabled"],[1,"btn","btn-primary","col-12","mb-2",3,"click","ngbTooltip","disabled"],[1,"control-container"],[1,"controls"],[1,"mb-3"],["for","library-type",1,"form-label"],["id","library-type","formControlName","bookReaderFontFamily",1,"form-select"],[3,"value"],[1,"row","g-0","controls"],["for","fontsize",1,"form-label","col-6"],[1,"col-6","float-end",2,"display","inline-flex"],[1,"fa-solid","fa-font",2,"font-size","0.75rem"],["type","range","id","fontsize","min","50","max","300","step","10","formControlName","bookReaderFontSize",1,"form-range","ms-2","me-2",3,"ngbTooltip"],[1,"fa-solid","fa-font",2,"font-size","1.5rem"],["for","linespacing",1,"form-label","col-6"],["type","range","id","linespacing","min","100","max","200","step","10","formControlName","bookReaderLineSpacing",1,"form-range","ms-2","me-2",3,"ngbTooltip"],["for","margin",1,"form-label","col-6"],[1,"fa-solid","fa-outdent"],["type","range","id","margin","min","0","max","30","step","5","formControlName","bookReaderMargin",1,"form-range","ms-2","me-2",3,"ngbTooltip"],[1,"fa-solid","fa-indent"],[1,"row","g-0","justify-content-between","mt-2"],[1,"btn","btn-primary","col",3,"click"],[1,"controls","d-flex","justify-content-between","align-items-center"],["id","readingdirection",1,"form-label"],["aria-labelledby","readingdirection",1,"btn","btn-icon",3,"click","title"],["aria-hidden","true"],[1,"phone-hidden"],[1,"controls",2,"display","flex","justify-content","space-between","align-items","center"],["for","writing-style",1,"form-label"],["aria-hidden","true","placement","top","role","button","tabindex","0","aria-describedby","writingStyle-help",1,"fa","fa-info-circle","ms-1",3,"ngbTooltip"],["id","writingStyle-help",1,"visually-hidden"],[3,"ngTemplateOutlet"],["id","writing-style","aria-labelledby","writingStyle-help",1,"btn","btn-icon",3,"click","title"],["for","tap-pagination",1,"form-label"],["aria-hidden","true","placement","top","role","button","tabindex","0","aria-describedby","tapPagination-help",1,"fa","fa-info-circle","ms-1",3,"ngbTooltip"],["id","tapPagination-help",1,"visually-hidden"],[1,"form-check","form-switch"],["type","checkbox","id","tap-pagination","formControlName","bookReaderTapToPaginate","aria-labelledby","tapPagination-help","switch","",1,"form-check-input"],["for","immersive-mode",1,"form-label"],["aria-hidden","true","placement","top","role","button","tabindex","0","aria-describedby","immersiveMode-help",1,"fa","fa-info-circle","ms-1",3,"ngbTooltip"],["id","immersiveMode-help",1,"visually-hidden"],["type","checkbox","id","immersive-mode","formControlName","bookReaderImmersiveMode","aria-labelledby","immersiveMode-help","switch","",1,"form-check-input"],["for","disable-bookmark-icon",1,"form-label"],["aria-hidden","true","placement","top","role","button","tabindex","0","aria-describedby","disable-bookmark-icon-help",1,"fa","fa-info-circle","ms-1",3,"ngbTooltip"],["id","disable-bookmark-icon-help",1,"visually-hidden"],["type","checkbox","id","disable-bookmark-icon","formControlName","bookReaderDisableBookmarkIcon","aria-labelledby","disable-bookmark-icon-help","switch","",1,"form-check-input"],["id","fullscreen",1,"form-label"],["aria-hidden","true","placement","top","role","button","tabindex","1","aria-describedby","fullscreen-help",1,"fa","fa-info-circle","ms-1",3,"ngbTooltip"],["id","fullscreen-help",1,"visually-hidden"],["aria-labelledby","fullscreen",1,"btn","btn-icon",3,"click"],[1,"ms-1"],["id","layout-mode",1,"form-label",2,"margin-bottom","0.5rem"],["aria-hidden","true","placement","top","role","button","tabindex","1","aria-describedby","layout-help",1,"fa","fa-info-circle","ms-1",3,"ngbTooltip"],["id","layout-help",1,"visually-hidden"],["role","group",1,"btn-group","d-flex","justify-content-center"],["type","radio","formControlName","bookReaderLayoutMode","id","layout-mode-default","autocomplete","off",1,"btn-check",3,"value"],["for","layout-mode-default",1,"btn","btn-outline-primary"],["type","radio","formControlName","bookReaderLayoutMode","id","layout-mode-col1","autocomplete","off",1,"btn-check",3,"value"],["for","layout-mode-col1",1,"btn","btn-outline-primary"],[3,"innerHTML"],["type","radio","formControlName","bookReaderLayoutMode","id","layout-mode-col2","autocomplete","off",1,"btn-check",3,"value"],["for","layout-mode-col2",1,"btn","btn-outline-primary"],[1,"btn","btn-icon","color",3,"ngClass"],[1,"btn","btn-icon","color",3,"click","ngClass"],[1,"dot",3,"ngStyle"]],template:function(o,n){o&1&&O(0,kt,1,1,"ng-container"),o&2&&B(n.readingProfile!==null?0:-1)},dependencies:[He,Me,Oe,Be,Re,Ee,Te,Ne,Pe,Ie,De,Fe,Ae,W,T,Ye,Ue,J,qe,Ve,xe,_e,ye,we,ke],styles:[`.controls[_ngcontent-%COMP%]{margin:.25rem 0}.controls[_ngcontent-%COMP%]   .form-select[_ngcontent-%COMP%]   option[_ngcontent-%COMP%]{background-color:var(--input-bg-color)}.controls[_ngcontent-%COMP%]   .form-label[_ngcontent-%COMP%]{margin:0}.controls[_ngcontent-%COMP%]   .btn.btn-icon[_ngcontent-%COMP%]{display:flex;width:50%;justify-content:center;align-items:center}.controls[_ngcontent-%COMP%]   .btn.btn-icon.color[_ngcontent-%COMP%]{display:unset;width:auto}.controls[_ngcontent-%COMP%]   .btn.btn-icon.color[_ngcontent-%COMP%]   .dot[_ngcontent-%COMP%]{height:1.5625rem;width:1.5625rem;border-radius:50%;margin:0 auto}.controls[_ngcontent-%COMP%]   .form-check.form-switch[_ngcontent-%COMP%]{width:50%;display:flex;justify-content:center}.controls[_ngcontent-%COMP%]   .form-check.form-switch[_ngcontent-%COMP%]   input[_ngcontent-%COMP%]{margin-right:.25rem}.active[_ngcontent-%COMP%]{border:1px solid var(--primary-color)}  .accordion-body{padding:.25rem 1rem 1rem!important}
/*# sourceMappingURL=reader-settings.component-MRMYWRDU.css.map */`],changeDetection:0})}}return t})();export{so as a,bo as b};
//# sourceMappingURL=chunk-TV5AFFV3.js.map

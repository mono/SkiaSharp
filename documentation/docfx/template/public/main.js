export default {
  defaultTheme: "auto",
  showLightbox: (image) =>
    !image.closest("a") && image.naturalWidth > 640 && image.naturalHeight > 360,
  configureHljs: (hljs) => {
    hljs.registerAliases(["xaml"], { languageName: "xml" })
  },
  start: () => {
    const article = document.querySelector("article")
    if (!article) {
      return
    }

    if (article.querySelectorAll(":scope > h2 > a").length >= 3) {
      document.body.classList.add("skia-hub")
    }

    if (location.pathname.includes("/releases/")) {
      document.body.classList.add("skia-releases")
    }

    article.querySelectorAll("pre").forEach((pre) => {
      const code = pre.querySelector("code")
      const language =
        [...(code?.classList ?? [])]
          .find((name) => name.startsWith("lang-"))
          ?.slice(5) ?? "code"

      pre.tabIndex = 0
      pre.setAttribute("role", "region")
      pre.setAttribute("aria-label", `${language.toUpperCase()} code sample`)
    })
  },
}

# Как сюда закоммитить код?
Сделайте форк, добавьте свои изменения туда и откройте pull request. Процесс более подробно описан по [ссылке](http://kbroman.org/github_tutorial/pages/fork.html).
# Общая информация
* _Фильтр Собеля_ - алгоритм выделения контуров изображения. Обработка изображения идет областями размером 3х3. В центре области находится обрабатываемый пиксель. Новое значение пикселя получается путем применения масок ([матриц свертки](http://habrahabr.ru/post/142818/)) к выделенной области обрабатываемого пикселя. В ссылках есть подробное описание принципа работы алгоритма, а также значений масок.
* _Фильтр бинаризации_ - служит для обесцвечивания границ, выделенных фильтром Собеля. Все цвета, близкие к чёрному, превращаются в чёрный, остальные - в белый. Чтобы определить расстояние между цветами используется преобразование RGB -> XYZ -> LAB.
* _Медианный фильтр_ - служит для размытия изображения и устранения шумов. В коде фильтра были добавлены варианты обработки изображения с помощью квадратной и ромбовидной областей обхода, а так же возможность изменения радиуса.

# ProTips©
* Если вы написали фильтр, и не знаете, куда его добавить, добавьте в конструктор класса `FilterViewModel`.

# Ссылки
* [Список дорожных знаков Беларуси](https://ru.wikipedia.org/wiki/%D0%94%D0%BE%D1%80%D0%BE%D0%B6%D0%BD%D1%8B%D0%B5_%D0%B7%D0%BD%D0%B0%D0%BA%D0%B8_%D0%91%D0%B5%D0%BB%D0%BE%D1%80%D1%83%D1%81%D1%81%D0%B8%D0%B8)
* [Быстрая работа с `Bitmap`](http://csharpexamples.com/fast-image-processing-c/)
* [Алгоритмы выделения контуров изображений (в частности оператор Собеля)](http://habrahabr.ru/post/114452/)
* [Медианный фильтр] (https://en.wikipedia.org/wiki/Median_filter)

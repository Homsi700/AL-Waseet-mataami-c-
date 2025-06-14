# دليل نشر وتثبيت نظام إدارة المطعم (.NET 8.0)

## متطلبات النظام

### متطلبات الأجهزة
- **المعالج**: Intel Core i3 أو ما يعادله (يوصى بـ i5 أو أعلى)
- **الذاكرة**: 4 جيجابايت RAM كحد أدنى (يوصى بـ 8 جيجابايت)
- **مساحة التخزين**: 500 ميجابايت للتطبيق + مساحة إضافية للبيانات
- **الشاشة**: دقة 1366×768 كحد أدنى (يوصى بشاشة تعمل باللمس)
- **الطابعة**: طابعة إيصالات حرارية متوافقة مع ESC/POS

### متطلبات البرمجيات
- **نظام التشغيل**: Windows 10 (إصدار 1809 أو أحدث) أو Windows 11
- **.NET**: يتم تضمين .NET 8.0 Runtime في حزمة التثبيت
- **قاعدة البيانات**: مدمجة (SQLite) - لا تتطلب تثبيت منفصل

## محتويات حزمة التثبيت

### ملفات التثبيت الرئيسية
- **FoodManagerSetup.exe** (18MB): برنامج التثبيت الرئيسي
- **restaurant_data_v8.db** (5MB): قاعدة البيانات الأولية
- **Documentation.zip**: حزمة الوثائق الشاملة

### مكونات البرنامج
- تطبيق نقطة البيع (POS)
- وحدة إدارة المخزون
- وحدة التقارير والإحصائيات
- وحدة إدارة المستخدمين
- وحدة النسخ الاحتياطي والاستعادة

## خطوات التثبيت

### 1. التثبيت الأساسي

1. **تشغيل برنامج التثبيت**:
   - قم بتشغيل ملف `FoodManagerSetup.exe`
   - اتبع تعليمات المعالج للتثبيت

2. **اختيار مجلد التثبيت**:
   - المسار الافتراضي: `C:\Program Files\FastFoodManagement`
   - يمكن تغيير المسار حسب الحاجة

3. **اختيار المكونات**:
   - تطبيق نقطة البيع (إلزامي)
   - وحدة إدارة المخزون (موصى به)
   - وحدة التقارير (موصى به)
   - أدوات إضافية (اختياري)

4. **إعداد قاعدة البيانات**:
   - استخدام قاعدة البيانات النموذجية (موصى به للبدء)
   - إنشاء قاعدة بيانات فارغة

### 2. الإعداد الأولي

1. **تشغيل البرنامج لأول مرة**:
   - انقر على أيقونة البرنامج على سطح المكتب
   - سيظهر معالج الإعداد الأولي

2. **إنشاء حساب المدير**:
   - أدخل اسم المستخدم وكلمة المرور للمدير
   - أدخل معلومات الاتصال الأساسية

3. **إعداد معلومات المطعم**:
   - اسم المطعم
   - العنوان
   - رقم الهاتف
   - الشعار (اختياري)

4. **إعداد الضرائب والعملة**:
   - اختيار العملة الافتراضية
   - تكوين نسبة ضريبة القيمة المضافة
   - إعدادات الفوترة الإلكترونية (اختياري)

### 3. إعداد الطابعة

1. **تثبيت برامج تشغيل الطابعة**:
   - قم بتثبيت برامج تشغيل الطابعة المرفقة مع الجهاز
   - تأكد من توصيل الطابعة وتشغيلها

2. **إعداد الطابعة في النظام**:
   - افتح النظام وانتقل إلى "الإعدادات" > "الطابعات"
   - انقر على "إضافة طابعة جديدة"
   - اختر نوع الطابعة وموديلها
   - قم بإعداد حجم الورق وخيارات الطباعة

3. **اختبار الطباعة**:
   - انقر على "طباعة اختبار"
   - تأكد من طباعة الإيصال بشكل صحيح

## الإعدادات المتقدمة

### 1. إعداد النسخ الاحتياطي

1. **تكوين جدول النسخ الاحتياطي**:
   - افتح "الإعدادات" > "النسخ الاحتياطي"
   - حدد وقت النسخ الاحتياطي التلقائي (يوصى بعد ساعات العمل)
   - حدد موقع تخزين النسخ الاحتياطية

2. **إعداد النسخ الاحتياطي السحابي** (اختياري):
   - انقر على "إعداد النسخ الاحتياطي السحابي"
   - أدخل بيانات اعتماد خدمة التخزين السحابي
   - اختبر الاتصال والنسخ الاحتياطي

### 2. إعداد المستخدمين والصلاحيات

1. **إنشاء مستخدمين جدد**:
   - افتح "الإعدادات" > "المستخدمون"
   - انقر على "إضافة مستخدم جديد"
   - أدخل معلومات المستخدم وحدد مستوى الصلاحيات

2. **تكوين مجموعات الصلاحيات**:
   - افتح "الإعدادات" > "الصلاحيات"
   - قم بتخصيص صلاحيات كل مجموعة
   - حفظ التغييرات

### 3. إعداد المنتجات والمخزون

1. **استيراد المنتجات**:
   - افتح "المخزون" > "استيراد"
   - اختر ملف Excel أو CSV يحتوي على قائمة المنتجات
   - اتبع معالج الاستيراد

2. **إعداد تنبيهات المخزون**:
   - افتح "المخزون" > "التنبيهات"
   - حدد الحد الأدنى لكل منتج
   - قم بتكوين طريقة الإشعار (تنبيه في النظام، بريد إلكتروني)

## تخصيص النظام

### 1. تخصيص واجهة المستخدم

1. **تغيير السمة**:
   - افتح "الإعدادات" > "المظهر"
   - اختر السمة (فاتحة، داكنة، مخصصة)
   - اختر الألوان الرئيسية

2. **تخصيص شاشة نقطة البيع**:
   - افتح "الإعدادات" > "نقطة البيع"
   - قم بتخصيص ترتيب الأزرار والفئات
   - قم بتخصيص شاشة الطلبات

### 2. تخصيص التقارير

1. **إعداد التقارير المفضلة**:
   - افتح "التقارير" > "إدارة التقارير"
   - حدد التقارير التي تظهر في القائمة الرئيسية
   - قم بتخصيص معلمات التقارير الافتراضية

2. **إنشاء تقارير مخصصة**:
   - افتح "التقارير" > "تقرير جديد"
   - اختر نوع التقرير وحقول البيانات
   - حدد تنسيق العرض والتصدير

### 3. تخصيص الإيصالات والفواتير

1. **تخصيص قالب الإيصال**:
   - افتح "الإعدادات" > "الطباعة" > "قوالب الإيصالات"
   - قم بتخصيص رأس وتذييل الإيصال
   - أضف شعار المطعم ومعلومات الاتصال

2. **تخصيص قالب الفاتورة**:
   - افتح "الإعدادات" > "الطباعة" > "قوالب الفواتير"
   - قم بتخصيص تنسيق الفاتورة
   - أضف الشروط والأحكام ومعلومات الضريبة

## استكشاف الأخطاء وإصلاحها

### 1. مشاكل التثبيت الشائعة

#### مشكلة: فشل التثبيت بسبب نقص الصلاحيات

**الحل**:
- قم بتشغيل برنامج التثبيت كمسؤول (Run as Administrator)
- تأكد من تعطيل برامج مكافحة الفيروسات مؤقتًا أثناء التثبيت

#### مشكلة: عدم القدرة على إنشاء قاعدة البيانات

**الحل**:
- تأكد من وجود مساحة كافية على القرص
- تأكد من امتلاك صلاحيات الكتابة في مجلد التثبيت
- تحقق من عدم وجود نسخة أخرى من قاعدة البيانات مفتوحة

### 2. مشاكل التشغيل الشائعة

#### مشكلة: بطء في أداء النظام

**الحل**:
- تحقق من توفر مساحة كافية على القرص الصلب
- قم بتشغيل أداة تحسين قاعدة البيانات من "الإعدادات" > "الصيانة"
- تأكد من عدم تشغيل تطبيقات أخرى تستهلك موارد النظام

#### مشكلة: فشل الاتصال بالطابعة

**الحل**:
- تأكد من تشغيل الطابعة وتوصيلها بشكل صحيح
- أعد تثبيت برامج تشغيل الطابعة
- تحقق من إعدادات الطابعة في النظام

## الصيانة الدورية

### 1. تحديث النظام

1. **تثبيت التحديثات**:
   - افتح "الإعدادات" > "التحديثات"
   - انقر على "التحقق من التحديثات"
   - قم بتثبيت التحديثات المتوفرة

2. **جدول التحديثات الموصى به**:
   - تحديثات الأمان: فور توفرها
   - تحديثات الميزات: بعد ساعات العمل

### 2. صيانة قاعدة البيانات

1. **تحسين أداء قاعدة البيانات**:
   - افتح "الإعدادات" > "الصيانة" > "تحسين قاعدة البيانات"
   - قم بتشغيل العملية أسبوعيًا

2. **تنظيف البيانات القديمة**:
   - افتح "الإعدادات" > "الصيانة" > "تنظيف البيانات"
   - حدد نوع البيانات وفترة الاحتفاظ
   - قم بتشغيل العملية شهريًا

### 3. النسخ الاحتياطي والاستعادة

1. **التحقق من النسخ الاحتياطي**:
   - افتح "الإعدادات" > "النسخ الاحتياطي" > "سجل النسخ"
   - تأكد من نجاح عمليات النسخ الاحتياطي التلقائية

2. **اختبار الاستعادة**:
   - قم بإجراء اختبار استعادة دوري للتأكد من سلامة النسخ الاحتياطية
   - افتح "الإعدادات" > "النسخ الاحتياطي" > "اختبار الاستعادة"

## الدعم الفني

### 1. موارد الدعم

- **الوثائق**: راجع ملفات الوثائق المرفقة في حزمة `Documentation.zip`
- **قاعدة المعرفة**: زيارة موقع الدعم الفني على `https://support.foodmanager.com`
- **الأسئلة الشائعة**: متوفرة في "مساعدة" > "الأسئلة الشائعة" داخل التطبيق

### 2. الاتصال بالدعم الفني

- **البريد الإلكتروني**: support@foodmanager.com
- **الهاتف**: +966-XX-XXXXXXX
- **ساعات العمل**: الأحد إلى الخميس، 9:00 صباحًا - 5:00 مساءً

### 3. طلب المساعدة من داخل التطبيق

1. انقر على "مساعدة" > "طلب دعم فني"
2. أدخل وصفًا للمشكلة
3. أرفق لقطات شاشة إن وجدت
4. انقر على "إرسال"

## الخاتمة

تهانينا على تثبيت نظام إدارة المطعم بنجاح! هذا الدليل يوفر المعلومات الأساسية للبدء باستخدام النظام. للحصول على معلومات أكثر تفصيلاً، يرجى الرجوع إلى دليل المستخدم الشامل المتوفر في حزمة الوثائق.

نتمنى لكم تجربة ناجحة ومثمرة مع نظام إدارة المطعم!
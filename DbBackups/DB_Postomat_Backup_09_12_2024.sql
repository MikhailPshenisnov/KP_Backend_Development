PGDMP      )        	        |         
   PostomatDB    16.2    16.2 )               0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false                       0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false                       0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false                        1262    16930 
   PostomatDB    DATABASE     �   CREATE DATABASE "PostomatDB" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'Russian_Russia.1251';
    DROP DATABASE "PostomatDB";
                postgres    false            �            1259    16956    Cells    TABLE     �   CREATE TABLE public."Cells" (
    "Id" uuid NOT NULL,
    "CellSize" integer NOT NULL,
    "PostomatId" uuid NOT NULL,
    "OrderId" uuid
);
    DROP TABLE public."Cells";
       public         heap    postgres    false            �            1259    16961    Logs    TABLE     	  CREATE TABLE public."Logs" (
    "Id" uuid NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Origin" character varying(128) NOT NULL,
    "Type" character varying(128) NOT NULL,
    "Title" character varying(128) NOT NULL,
    "Message" text NOT NULL
);
    DROP TABLE public."Logs";
       public         heap    postgres    false            �            1259    16946 
   OrderPlans    TABLE     �  CREATE TABLE public."OrderPlans" (
    "Id" uuid NOT NULL,
    "Status" character varying(128) NOT NULL,
    "LastStatusChangeDate" timestamp with time zone NOT NULL,
    "StoreUntilDate" timestamp with time zone,
    "DeliveryCodeHash" character varying(128) NOT NULL,
    "OrderId" uuid NOT NULL,
    "PostomatId" uuid NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "DeliveredBy" uuid,
    "DeliveredBackBy" uuid
);
     DROP TABLE public."OrderPlans";
       public         heap    postgres    false            �            1259    16941    Orders    TABLE     �   CREATE TABLE public."Orders" (
    "Id" uuid NOT NULL,
    "ReceivingCodeHash" character varying(128) NOT NULL,
    "OrderSize" integer NOT NULL
);
    DROP TABLE public."Orders";
       public         heap    postgres    false            �            1259    16951 	   Postomats    TABLE     �   CREATE TABLE public."Postomats" (
    "Id" uuid NOT NULL,
    "Name" character varying(128) NOT NULL,
    "Address" character varying(128) NOT NULL
);
    DROP TABLE public."Postomats";
       public         heap    postgres    false            �            1259    16936    Roles    TABLE     �   CREATE TABLE public."Roles" (
    "Id" uuid NOT NULL,
    "RoleName" character varying(128) NOT NULL,
    "AccessLvl" integer NOT NULL
);
    DROP TABLE public."Roles";
       public         heap    postgres    false            �            1259    16931    Users    TABLE     �   CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "Login" character varying(128) NOT NULL,
    "PasswordHash" character varying(128) NOT NULL,
    "RoleId" uuid NOT NULL
);
    DROP TABLE public."Users";
       public         heap    postgres    false                      0    16956    Cells 
   TABLE DATA           L   COPY public."Cells" ("Id", "CellSize", "PostomatId", "OrderId") FROM stdin;
    public          postgres    false    220   �1                 0    16961    Logs 
   TABLE DATA           T   COPY public."Logs" ("Id", "Date", "Origin", "Type", "Title", "Message") FROM stdin;
    public          postgres    false    221   2                 0    16946 
   OrderPlans 
   TABLE DATA           �   COPY public."OrderPlans" ("Id", "Status", "LastStatusChangeDate", "StoreUntilDate", "DeliveryCodeHash", "OrderId", "PostomatId", "CreatedBy", "DeliveredBy", "DeliveredBackBy") FROM stdin;
    public          postgres    false    218   %2                 0    16941    Orders 
   TABLE DATA           J   COPY public."Orders" ("Id", "ReceivingCodeHash", "OrderSize") FROM stdin;
    public          postgres    false    217   B2                 0    16951 	   Postomats 
   TABLE DATA           >   COPY public."Postomats" ("Id", "Name", "Address") FROM stdin;
    public          postgres    false    219   _2                 0    16936    Roles 
   TABLE DATA           @   COPY public."Roles" ("Id", "RoleName", "AccessLvl") FROM stdin;
    public          postgres    false    216   |2                 0    16931    Users 
   TABLE DATA           J   COPY public."Users" ("Id", "Login", "PasswordHash", "RoleId") FROM stdin;
    public          postgres    false    215   �2       x           2606    16960    Cells Cells_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public."Cells"
    ADD CONSTRAINT "Cells_pkey" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Cells" DROP CONSTRAINT "Cells_pkey";
       public            postgres    false    220            |           2606    16967    Logs Logs_pkey 
   CONSTRAINT     R   ALTER TABLE ONLY public."Logs"
    ADD CONSTRAINT "Logs_pkey" PRIMARY KEY ("Id");
 <   ALTER TABLE ONLY public."Logs" DROP CONSTRAINT "Logs_pkey";
       public            postgres    false    221            o           2606    16950    OrderPlans OrderPlans_pkey 
   CONSTRAINT     ^   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_pkey" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_pkey";
       public            postgres    false    218            m           2606    16945    Orders Orders_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public."Orders"
    ADD CONSTRAINT "Orders_pkey" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."Orders" DROP CONSTRAINT "Orders_pkey";
       public            postgres    false    217            v           2606    16955    Postomats Postomats_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY public."Postomats"
    ADD CONSTRAINT "Postomats_pkey" PRIMARY KEY ("Id");
 F   ALTER TABLE ONLY public."Postomats" DROP CONSTRAINT "Postomats_pkey";
       public            postgres    false    219            k           2606    16940    Roles Roles_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "Roles_pkey" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Roles" DROP CONSTRAINT "Roles_pkey";
       public            postgres    false    216            h           2606    16935    Users Users_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Users" DROP CONSTRAINT "Users_pkey";
       public            postgres    false    215            y           1259    16978    fki_Cells_OrderId_fkey    INDEX     Q   CREATE INDEX "fki_Cells_OrderId_fkey" ON public."Cells" USING btree ("OrderId");
 ,   DROP INDEX public."fki_Cells_OrderId_fkey";
       public            postgres    false    220            z           1259    16984    fki_Cells_PostomatId_fkey    INDEX     W   CREATE INDEX "fki_Cells_PostomatId_fkey" ON public."Cells" USING btree ("PostomatId");
 /   DROP INDEX public."fki_Cells_PostomatId_fkey";
       public            postgres    false    220            p           1259    17002    fki_OrderPlans_CreatedBy_fkey    INDEX     _   CREATE INDEX "fki_OrderPlans_CreatedBy_fkey" ON public."OrderPlans" USING btree ("CreatedBy");
 3   DROP INDEX public."fki_OrderPlans_CreatedBy_fkey";
       public            postgres    false    218            q           1259    17014 #   fki_OrderPlans_DeliveredBackBy_fkey    INDEX     k   CREATE INDEX "fki_OrderPlans_DeliveredBackBy_fkey" ON public."OrderPlans" USING btree ("DeliveredBackBy");
 9   DROP INDEX public."fki_OrderPlans_DeliveredBackBy_fkey";
       public            postgres    false    218            r           1259    17008    fki_OrderPlans_DeliveredBy_fkey    INDEX     c   CREATE INDEX "fki_OrderPlans_DeliveredBy_fkey" ON public."OrderPlans" USING btree ("DeliveredBy");
 5   DROP INDEX public."fki_OrderPlans_DeliveredBy_fkey";
       public            postgres    false    218            s           1259    16990    fki_OrderPlans_OrderId_fkey    INDEX     [   CREATE INDEX "fki_OrderPlans_OrderId_fkey" ON public."OrderPlans" USING btree ("OrderId");
 1   DROP INDEX public."fki_OrderPlans_OrderId_fkey";
       public            postgres    false    218            t           1259    16996    fki_OrderPlans_PostomatId_fkey    INDEX     a   CREATE INDEX "fki_OrderPlans_PostomatId_fkey" ON public."OrderPlans" USING btree ("PostomatId");
 4   DROP INDEX public."fki_OrderPlans_PostomatId_fkey";
       public            postgres    false    218            i           1259    17020    fki_Users_RoleId_fkey    INDEX     O   CREATE INDEX "fki_Users_RoleId_fkey" ON public."Users" USING btree ("RoleId");
 +   DROP INDEX public."fki_Users_RoleId_fkey";
       public            postgres    false    215            �           2606    16973    Cells Cells_OrderId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."Cells"
    ADD CONSTRAINT "Cells_OrderId_fkey" FOREIGN KEY ("OrderId") REFERENCES public."Orders"("Id");
 F   ALTER TABLE ONLY public."Cells" DROP CONSTRAINT "Cells_OrderId_fkey";
       public          postgres    false    4717    220    217            �           2606    16979    Cells Cells_PostomatId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."Cells"
    ADD CONSTRAINT "Cells_PostomatId_fkey" FOREIGN KEY ("PostomatId") REFERENCES public."Postomats"("Id");
 I   ALTER TABLE ONLY public."Cells" DROP CONSTRAINT "Cells_PostomatId_fkey";
       public          postgres    false    220    219    4726            ~           2606    16997 $   OrderPlans OrderPlans_CreatedBy_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_CreatedBy_fkey" FOREIGN KEY ("CreatedBy") REFERENCES public."Users"("Id");
 R   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_CreatedBy_fkey";
       public          postgres    false    215    4712    218                       2606    17009 *   OrderPlans OrderPlans_DeliveredBackBy_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_DeliveredBackBy_fkey" FOREIGN KEY ("DeliveredBackBy") REFERENCES public."Users"("Id");
 X   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_DeliveredBackBy_fkey";
       public          postgres    false    215    218    4712            �           2606    17003 &   OrderPlans OrderPlans_DeliveredBy_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_DeliveredBy_fkey" FOREIGN KEY ("DeliveredBy") REFERENCES public."Users"("Id");
 T   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_DeliveredBy_fkey";
       public          postgres    false    218    215    4712            �           2606    16985 "   OrderPlans OrderPlans_OrderId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_OrderId_fkey" FOREIGN KEY ("OrderId") REFERENCES public."Orders"("Id");
 P   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_OrderId_fkey";
       public          postgres    false    218    217    4717            �           2606    16991 %   OrderPlans OrderPlans_PostomatId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_PostomatId_fkey" FOREIGN KEY ("PostomatId") REFERENCES public."Postomats"("Id");
 S   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_PostomatId_fkey";
       public          postgres    false    219    4726    218            }           2606    17015    Users Users_RoleId_fkey    FK CONSTRAINT        ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_RoleId_fkey" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id");
 E   ALTER TABLE ONLY public."Users" DROP CONSTRAINT "Users_RoleId_fkey";
       public          postgres    false    4715    216    215                  x������ � �            x������ � �            x������ � �            x������ � �            x������ � �            x������ � �            x������ � �     
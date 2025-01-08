PGDMP                       }         
   PostomatDB    16.6 (Debian 16.6-1.pgdg120+1)    16.6 )    P           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            Q           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            R           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            S           1262    16384 
   PostomatDB    DATABASE     w   CREATE DATABASE "PostomatDB" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'en_US.utf8';
    DROP DATABASE "PostomatDB";
                postgres    false            �            1259    16385    Cells    TABLE     �   CREATE TABLE public."Cells" (
    "Id" uuid NOT NULL,
    "CellSize" integer NOT NULL,
    "PostomatId" uuid NOT NULL,
    "OrderId" uuid
);
    DROP TABLE public."Cells";
       public         heap    postgres    false            �            1259    16388    Logs    TABLE     	  CREATE TABLE public."Logs" (
    "Id" uuid NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Origin" character varying(256) NOT NULL,
    "Type" character varying(256) NOT NULL,
    "Title" character varying(256) NOT NULL,
    "Message" text NOT NULL
);
    DROP TABLE public."Logs";
       public         heap    postgres    false            �            1259    16393 
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
       public         heap    postgres    false            �            1259    16396    Orders    TABLE     �   CREATE TABLE public."Orders" (
    "Id" uuid NOT NULL,
    "ReceivingCodeHash" character varying(128) NOT NULL,
    "OrderSize" integer NOT NULL
);
    DROP TABLE public."Orders";
       public         heap    postgres    false            �            1259    16399 	   Postomats    TABLE     �   CREATE TABLE public."Postomats" (
    "Id" uuid NOT NULL,
    "Name" character varying(128) NOT NULL,
    "Address" character varying(128) NOT NULL
);
    DROP TABLE public."Postomats";
       public         heap    postgres    false            �            1259    16402    Roles    TABLE     �   CREATE TABLE public."Roles" (
    "Id" uuid NOT NULL,
    "RoleName" character varying(128) NOT NULL,
    "AccessLvl" integer NOT NULL
);
    DROP TABLE public."Roles";
       public         heap    postgres    false            �            1259    16405    Users    TABLE     �   CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "Login" character varying(128) NOT NULL,
    "PasswordHash" character varying(128) NOT NULL,
    "RoleId" uuid NOT NULL
);
    DROP TABLE public."Users";
       public         heap    postgres    false            G          0    16385    Cells 
   TABLE DATA           L   COPY public."Cells" ("Id", "CellSize", "PostomatId", "OrderId") FROM stdin;
    public          postgres    false    215   �1       H          0    16388    Logs 
   TABLE DATA           T   COPY public."Logs" ("Id", "Date", "Origin", "Type", "Title", "Message") FROM stdin;
    public          postgres    false    216   2       I          0    16393 
   OrderPlans 
   TABLE DATA           �   COPY public."OrderPlans" ("Id", "Status", "LastStatusChangeDate", "StoreUntilDate", "DeliveryCodeHash", "OrderId", "PostomatId", "CreatedBy", "DeliveredBy", "DeliveredBackBy") FROM stdin;
    public          postgres    false    217   �4       J          0    16396    Orders 
   TABLE DATA           J   COPY public."Orders" ("Id", "ReceivingCodeHash", "OrderSize") FROM stdin;
    public          postgres    false    218   �4       K          0    16399 	   Postomats 
   TABLE DATA           >   COPY public."Postomats" ("Id", "Name", "Address") FROM stdin;
    public          postgres    false    219   �4       L          0    16402    Roles 
   TABLE DATA           @   COPY public."Roles" ("Id", "RoleName", "AccessLvl") FROM stdin;
    public          postgres    false    220   5       M          0    16405    Users 
   TABLE DATA           J   COPY public."Users" ("Id", "Login", "PasswordHash", "RoleId") FROM stdin;
    public          postgres    false    221   �5       �           2606    16409    Cells Cells_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public."Cells"
    ADD CONSTRAINT "Cells_pkey" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Cells" DROP CONSTRAINT "Cells_pkey";
       public            postgres    false    215            �           2606    16411    Logs Logs_pkey 
   CONSTRAINT     R   ALTER TABLE ONLY public."Logs"
    ADD CONSTRAINT "Logs_pkey" PRIMARY KEY ("Id");
 <   ALTER TABLE ONLY public."Logs" DROP CONSTRAINT "Logs_pkey";
       public            postgres    false    216            �           2606    16413    OrderPlans OrderPlans_pkey 
   CONSTRAINT     ^   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_pkey" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_pkey";
       public            postgres    false    217            �           2606    16415    Orders Orders_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public."Orders"
    ADD CONSTRAINT "Orders_pkey" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."Orders" DROP CONSTRAINT "Orders_pkey";
       public            postgres    false    218            �           2606    16417    Postomats Postomats_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY public."Postomats"
    ADD CONSTRAINT "Postomats_pkey" PRIMARY KEY ("Id");
 F   ALTER TABLE ONLY public."Postomats" DROP CONSTRAINT "Postomats_pkey";
       public            postgres    false    219            �           2606    16419    Roles Roles_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "Roles_pkey" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Roles" DROP CONSTRAINT "Roles_pkey";
       public            postgres    false    220            �           2606    16421    Users Users_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Users" DROP CONSTRAINT "Users_pkey";
       public            postgres    false    221            �           1259    16422    fki_Cells_OrderId_fkey    INDEX     Q   CREATE INDEX "fki_Cells_OrderId_fkey" ON public."Cells" USING btree ("OrderId");
 ,   DROP INDEX public."fki_Cells_OrderId_fkey";
       public            postgres    false    215            �           1259    16423    fki_Cells_PostomatId_fkey    INDEX     W   CREATE INDEX "fki_Cells_PostomatId_fkey" ON public."Cells" USING btree ("PostomatId");
 /   DROP INDEX public."fki_Cells_PostomatId_fkey";
       public            postgres    false    215            �           1259    16424    fki_OrderPlans_CreatedBy_fkey    INDEX     _   CREATE INDEX "fki_OrderPlans_CreatedBy_fkey" ON public."OrderPlans" USING btree ("CreatedBy");
 3   DROP INDEX public."fki_OrderPlans_CreatedBy_fkey";
       public            postgres    false    217            �           1259    16425 #   fki_OrderPlans_DeliveredBackBy_fkey    INDEX     k   CREATE INDEX "fki_OrderPlans_DeliveredBackBy_fkey" ON public."OrderPlans" USING btree ("DeliveredBackBy");
 9   DROP INDEX public."fki_OrderPlans_DeliveredBackBy_fkey";
       public            postgres    false    217            �           1259    16426    fki_OrderPlans_DeliveredBy_fkey    INDEX     c   CREATE INDEX "fki_OrderPlans_DeliveredBy_fkey" ON public."OrderPlans" USING btree ("DeliveredBy");
 5   DROP INDEX public."fki_OrderPlans_DeliveredBy_fkey";
       public            postgres    false    217            �           1259    16427    fki_OrderPlans_OrderId_fkey    INDEX     [   CREATE INDEX "fki_OrderPlans_OrderId_fkey" ON public."OrderPlans" USING btree ("OrderId");
 1   DROP INDEX public."fki_OrderPlans_OrderId_fkey";
       public            postgres    false    217            �           1259    16428    fki_OrderPlans_PostomatId_fkey    INDEX     a   CREATE INDEX "fki_OrderPlans_PostomatId_fkey" ON public."OrderPlans" USING btree ("PostomatId");
 4   DROP INDEX public."fki_OrderPlans_PostomatId_fkey";
       public            postgres    false    217            �           1259    16429    fki_Users_RoleId_fkey    INDEX     O   CREATE INDEX "fki_Users_RoleId_fkey" ON public."Users" USING btree ("RoleId");
 +   DROP INDEX public."fki_Users_RoleId_fkey";
       public            postgres    false    221            �           2606    16430    Cells Cells_OrderId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."Cells"
    ADD CONSTRAINT "Cells_OrderId_fkey" FOREIGN KEY ("OrderId") REFERENCES public."Orders"("Id");
 F   ALTER TABLE ONLY public."Cells" DROP CONSTRAINT "Cells_OrderId_fkey";
       public          postgres    false    218    3240    215            �           2606    16435    Cells Cells_PostomatId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."Cells"
    ADD CONSTRAINT "Cells_PostomatId_fkey" FOREIGN KEY ("PostomatId") REFERENCES public."Postomats"("Id");
 I   ALTER TABLE ONLY public."Cells" DROP CONSTRAINT "Cells_PostomatId_fkey";
       public          postgres    false    219    215    3242            �           2606    16440 $   OrderPlans OrderPlans_CreatedBy_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_CreatedBy_fkey" FOREIGN KEY ("CreatedBy") REFERENCES public."Users"("Id");
 R   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_CreatedBy_fkey";
       public          postgres    false    221    217    3246            �           2606    16445 *   OrderPlans OrderPlans_DeliveredBackBy_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_DeliveredBackBy_fkey" FOREIGN KEY ("DeliveredBackBy") REFERENCES public."Users"("Id");
 X   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_DeliveredBackBy_fkey";
       public          postgres    false    217    3246    221            �           2606    16450 &   OrderPlans OrderPlans_DeliveredBy_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_DeliveredBy_fkey" FOREIGN KEY ("DeliveredBy") REFERENCES public."Users"("Id");
 T   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_DeliveredBy_fkey";
       public          postgres    false    3246    221    217            �           2606    16455 "   OrderPlans OrderPlans_OrderId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_OrderId_fkey" FOREIGN KEY ("OrderId") REFERENCES public."Orders"("Id");
 P   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_OrderId_fkey";
       public          postgres    false    217    218    3240            �           2606    16460 %   OrderPlans OrderPlans_PostomatId_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public."OrderPlans"
    ADD CONSTRAINT "OrderPlans_PostomatId_fkey" FOREIGN KEY ("PostomatId") REFERENCES public."Postomats"("Id");
 S   ALTER TABLE ONLY public."OrderPlans" DROP CONSTRAINT "OrderPlans_PostomatId_fkey";
       public          postgres    false    3242    217    219            �           2606    16465    Users Users_RoleId_fkey    FK CONSTRAINT        ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "Users_RoleId_fkey" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id");
 E   ALTER TABLE ONLY public."Users" DROP CONSTRAINT "Users_RoleId_fkey";
       public          postgres    false    3244    220    221            G      x������ � �      H   �  x��T�n�0<�_A�Ԣ]�O�ԡ@���S�4����2�H�$'i����Į[$F/��PK�h�[��{�48m"Hk<�(x�JƹBö́S.�q��0SQ]q]����Q:9Y�����n�f�����i׵ە�/�%�;��c���"C{���q>�CK�p �;2SΫȼ� �Y*��\:L>0+=�"���i�MW�#;g�@K�P �)H��yf0���+��f;j��Z�ך�M����w�Զ���W��K���$�u��jUN9z U����Pn�?*�X��il�'M;���CүR�C��@\�����C?�E靆�A�t���%�Rr�I��Ce@5�#Ê��3��ڏ���7'g���پ����vXq���_F�o��s���~���{O�	�\�C~1f��C��Ƽ��8}x;���G��^�c�NNn1	����������1l�[Zdf��OH��Q �p�E�*�R�1Si�x�&sO���w�4�2[�DʋT����1_/����;K��R"߲B&��o���Kâ�<	1N3�GF*���;����0ե
�G*�H�x�����"rqh��+X��=k�'\f�ݏ=K���������'���n	x�5GuX���8��v|}��}�VL��_ƘE      I      x������ � �      J      x������ � �      K      x������ � �      L   e   x�5�1� ���Q
&�רԭߘ)R��ܿS��2�MQ0�5hcә���.9fZ�7�{�����vU�+*���"���a(�GAm	}��̌�� �(      M   �   x��K�0 �5=�:Li{~��C�0�5D�����]ࡶJZm�C ��(h;G�����2�o�y}�i�����R�Y����˲�*%�%��W����rL��)9�����f��.ݏ�!;�00Z�F�5�-��oT�H�?�+     